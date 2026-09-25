using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Payments.Commands.WebHook;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace AlMostashar.Application.Tests;

public class WebHookCommandHandlerTests
{
    private const string SecretHmac = "TestSecretHmac";

    [Fact]
    public async Task WebHook_AmountMismatch_FromPending_DoesNotMarkSucceeded()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var seed = await SeedPaymentAsync(db, PaymentStatus.Pending, amount: 500m);

        var mediatorMock = new Mock<IMediator>();
        var handler = CreateHandler(db, mediatorMock.Object);

        var command = CreateCommand(seed.Payment, isSuccess: true, webhookAmountEgp: 999m); // Amount mismatch

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Payment success skipped due to amount mismatch.", result.Value);

        var payment = await db.Payments.SingleAsync();
        Assert.Equal(PaymentStatus.Pending, payment.Status); // Remains Pending

        var invoice = await db.Invoices.SingleAsync();
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);

        mediatorMock.Verify(m => m.Publish(It.IsAny<InvoicePaidEvent>(), It.IsAny<CancellationToken>()), Times.Never);

        var log = await db.WebhookLogs.SingleAsync();
        Assert.Equal("Error", log.ProcessingResult);
        Assert.Contains("Amount mismatch", log.ErrorMessage);
    }

    [Fact]
    public async Task WebHook_AmountMismatch_FromFailed_RemainsFailed()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var seed = await SeedPaymentAsync(db, PaymentStatus.Failed, amount: 200m);

        var mediatorMock = new Mock<IMediator>();
        var handler = CreateHandler(db, mediatorMock.Object);

        var command = CreateCommand(seed.Payment, isSuccess: true, webhookAmountEgp: 201m); // Mismatch

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Payment success skipped due to amount mismatch.", result.Value);

        var payment = await db.Payments.SingleAsync();
        Assert.Equal(PaymentStatus.Failed, payment.Status); // Must NOT become Pending
    }

    [Fact]
    public async Task WebHook_MalformedHmac_ReturnsFailure()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var seed = await SeedPaymentAsync(db, PaymentStatus.Pending, amount: 100m);
        var handler = CreateHandler(db, Mock.Of<IMediator>());

        var command = CreateCommand(seed.Payment, isSuccess: true, webhookAmountEgp: 100m);
        command.Hmac = "NOT_A_HEX_STRING";

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Webhook.InvalidSignature", result.Error?.Code);

        var payment = await db.Payments.SingleAsync();
        Assert.Equal(PaymentStatus.Pending, payment.Status);

        var log = await db.WebhookLogs.SingleAsync();
        Assert.Equal("Error", log.ProcessingResult);
        Assert.Contains("Invalid HMAC", log.ErrorMessage);
    }

    [Fact]
    public async Task WebHook_DuplicateSucceeded_RecoversEscrow_ButDoesNotPublishEvent()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var seed = await SeedPaymentAsync(db, PaymentStatus.Succeeded, amount: 150m, invoiceStatus: InvoiceStatus.Paid);
        // Escrow missing

        var mediatorMock = new Mock<IMediator>();
        var escrowFundingMock = new Mock<IEscrowFundingService>();

        var handler = new WebHookCommandHandler(
            db,
            mediatorMock.Object,
            CreateConfiguration(),
            Mock.Of<INotificationService>(),
            escrowFundingMock.Object,
            NullLogger<WebHookCommandHandler>.Instance);

        var command = CreateCommand(seed.Payment, isSuccess: true, webhookAmountEgp: 150m);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("already in", result.Value);

        // Verify recovery was called
        escrowFundingMock.Verify(s => s.FundEscrowAsync(seed.Invoice.ClientRequestId, seed.Payment.Id, 150m, It.IsAny<CancellationToken>()), Times.Once);

        // Verify no duplicate publish
        mediatorMock.Verify(m => m.Publish(It.IsAny<InvoicePaidEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ──────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────

    private WebHookCommandHandler CreateHandler(IAppDbContext db, IMediator mediator)
    {
        return new WebHookCommandHandler(
            db,
            mediator,
            CreateConfiguration(),
            Mock.Of<INotificationService>(),
            Mock.Of<IEscrowFundingService>(),
            NullLogger<WebHookCommandHandler>.Instance);
    }

    private static IConfiguration CreateConfiguration()
    {
        var configParams = new Dictionary<string, string> { { "PaymobSettings:HMAC", SecretHmac } };
        return new ConfigurationBuilder().AddInMemoryCollection(configParams!).Build();
    }

    private static async Task<(Payment Payment, Invoice Invoice)> SeedPaymentAsync(IAppDbContext db, PaymentStatus status, decimal amount, InvoiceStatus invoiceStatus = InvoiceStatus.Unpaid)
    {
        var client = TestEntityFactory.CreateClient("999");
        var lawyer = TestEntityFactory.CreateLawyer("999");
        db.Clients.Add(client);
        db.Lawyers.Add(lawyer);
        await db.SaveChangesAsync(CancellationToken.None);

        var request = new DirectRequest
        {
            RequestId = "REQ-WH",
            Title = "WH",
            ClientId = client.Id,
            LawyerServiceLawyerId = lawyer.Id,
            Status = ClientRequestStatus.InProgress,
            ProblemDetails = "Some issue"
        };
        db.ClientRequests.Add(request);
        await db.SaveChangesAsync(CancellationToken.None);

        var invoice = new Invoice
        {
            ReferenceNumber = "INV-WH",
            PlatformFee = 0,
            LawyerAmount = amount,
            TotalAmount = amount,
            Status = invoiceStatus,
            ClientRequestId = request.Id
        };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(CancellationToken.None);

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            Amount = amount,
            Status = status,
            PayDate = DateTime.UtcNow,
            PaymentProvider = PaymentProvider.Paymob
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync(CancellationToken.None);

        return (payment, invoice);
    }

    private static WebHookCommand CreateCommand(Payment payment, bool isSuccess, decimal webhookAmountEgp)
    {
        var obj = new PaymobTransactionObjDto
        {
            id = 12345,
            amount_cents = (int)(webhookAmountEgp * 100),
            success = isSuccess,
            pending = !isSuccess,
            created_at = DateTime.UtcNow.ToString("O"),
            currency = "EGP",
            order = new PaymobOrderDto { id = 54321, merchant_order_id = $"{payment.Id}_guid" },
            source_data = new PaymobSourceDataDto { type = "card", pan = "1234", sub_type = "MasterCard" }
        };

        var command = new WebHookCommand
        {
            Payload = new PaymobWebHookDto
            {
                type = "TRANSACTION",
                obj = obj
            },
            GatewayResponse = "{}",
            Hmac = GenerateValidHmac(obj, SecretHmac)
        };

        return command;
    }

    private static string GenerateValidHmac(PaymobTransactionObjDto obj, string secret)
    {
        string concatenatedString =
            $"{obj.amount_cents}" +
            $"{obj.created_at}" +
            $"{obj.currency}" +
            $"{obj.error_occured.ToString().ToLower()}" +
            $"{obj.has_parent_transaction.ToString().ToLower()}" +
            $"{obj.id}" +
            $"{obj.integration_id}" +
            $"{obj.is_3d_secure.ToString().ToLower()}" +
            $"{obj.is_auth.ToString().ToLower()}" +
            $"{obj.is_capture.ToString().ToLower()}" +
            $"{obj.is_refunded.ToString().ToLower()}" +
            $"{obj.is_standalone_payment.ToString().ToLower()}" +
            $"{obj.is_voided.ToString().ToLower()}" +
            $"{obj.order?.id}" +
            $"{obj.owner}" +
            $"{obj.pending.ToString().ToLower()}" +
            $"{obj.source_data?.pan}" +
            $"{obj.source_data?.sub_type}" +
            $"{obj.source_data?.type}" +
            $"{obj.success.ToString().ToLower()}";

        using var hmacSha512 = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
        byte[] computedHash = hmacSha512.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString));
        return Convert.ToHexString(computedHash).ToLower();
    }
}
