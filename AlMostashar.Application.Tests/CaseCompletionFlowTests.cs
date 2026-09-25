using AlMostashar.Application.Features.Cases.Commands.ConfirmCaseCompletion;
using AlMostashar.Application.Features.Cases.Commands.FinishCase;
using AlMostashar.Application.Features.Cases.Commands.CancelCase;
using AlMostashar.Application.Features.Cases.Commands.StartCase;
using AlMostashar.Application.Features.Cases.Queries.GetCaseById;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.Services;
using Microsoft.Extensions.Logging.Abstractions;
using MediatR;
using AlMostashar.Domain.Shared;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Payments.Commands.RefundPayment;
using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Application.Features.Escrows.Commands.RefundEscrow;

namespace AlMostashar.Application.Tests;

public class CaseCompletionFlowTests
{
    [Fact]
    public async Task LawyerCompletesPlatformCase_MovesToPendingConfirmation()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedPlatformCaseAsync(scope.DbContext, "201", CaseStatus.InProgress);

        var handler = new FinishCaseCommandHandler(
            scope.DbContext,
            new FixedCurrentUserService(seed.Lawyer.Id),
            Mock.Of<INotificationService>(),
            Mock.Of<ILogger<FinishCaseCommandHandler>>());

        var pendingResult = await handler.Handle(
            new FinishCaseCommand(seed.Case.Id),
            CancellationToken.None);

        Assert.True(pendingResult.IsSuccess);
        Assert.Equal(CaseStatus.PendingConfirmation, seed.Case.Status);
        Assert.Equal(ClientRequestStatus.InProgress, seed.ClientRequest.Status);
    }

    [Fact]
    public async Task LawyerStartsPlatformCase_MovesToInProgress()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedPlatformCaseAsync(scope.DbContext, "204", CaseStatus.Open);

        var handler = new StartCaseCommandHandler(
            scope.DbContext,
            new FixedCurrentUserService(seed.Lawyer.Id),
            Mock.Of<INotificationService>(),
            Mock.Of<ILogger<StartCaseCommandHandler>>());

        var startResult = await handler.Handle(
            new StartCaseCommand(seed.Case.Id),
            CancellationToken.None);

        Assert.True(startResult.IsSuccess);
        Assert.Equal(CaseStatus.InProgress, seed.Case.Status);
        Assert.Equal(ClientRequestStatus.InProgress, seed.ClientRequest.Status);
    }

    [Fact]
    public async Task LawyerCancelsPlatformCase_MovesToCanceled()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedPlatformCaseAsync(scope.DbContext, "205", CaseStatus.InProgress);

        var handler = new CancelCaseCommandHandler(
            scope.DbContext,
            new FixedCurrentUserService(seed.Lawyer.Id),
            Mock.Of<INotificationService>(),
            Mock.Of<IMediator>(),
            Mock.Of<ILogger<CancelCaseCommandHandler>>());

        var cancelResult = await handler.Handle(
            new CancelCaseCommand(seed.Case.Id, "Client requested cancellation"),
            CancellationToken.None);

        Assert.True(cancelResult.IsSuccess);
        Assert.Equal(CaseStatus.Canceled, seed.Case.Status);
        Assert.Equal("Client requested cancellation", seed.Case.CancellationReason);
        Assert.Equal(ClientRequestStatus.Cancelled, seed.ClientRequest.Status);
    }

    [Fact]
    public async Task LawyerCancelsPlatformCase_WithFundedEscrow_RefundsPaymentAndEscrow()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedPlatformCaseAsync(scope.DbContext, "206", CaseStatus.InProgress);

        var invoice = new Invoice
        {
            ReferenceNumber = $"INV-CONF-206",
            PlatformFee = 15m,
            LawyerAmount = 135m,
            TotalAmount = 150m,
            Status = InvoiceStatus.Paid,
            ClientRequestId = seed.ClientRequest.Id
        };
        scope.DbContext.Invoices.Add(invoice);
        await scope.DbContext.SaveChangesAsync();

        var payment = new Payment
        {
            Amount = 150m,
            Currency = "EGP",
            PayDate = DateTime.UtcNow,
            Status = PaymentStatus.Succeeded,
            TransactionId = 12345,
            InvoiceId = invoice.Id
        };
        scope.DbContext.Payments.Add(payment);
        await scope.DbContext.SaveChangesAsync();

        var escrow = new Escrow
        {
            RequestId = seed.ClientRequest.Id,
            Amount = 150m,
            Status = EscrowStatus.Funded,
            PaymentId = payment.Id,
            EscrowFundedAt = DateTime.UtcNow
        };
        scope.DbContext.Escrows.Add(escrow);
        await scope.DbContext.SaveChangesAsync();

        var mediatorMock = new Mock<IMediator>();

        var refundPaymentResult = Result<RefundPaymentResultDto>.Success(new RefundPaymentResultDto
        {
            RefundTransactionId = 999,
            AmountCents = 15000,
            GatewayMessage = "Refunded",
            RawResponse = "{}"
        });

        mediatorMock.Setup(m => m.Send(
            It.Is<RefundPaymentCommand>(cmd => cmd.PaymentId == payment.Id && cmd.RefundedAmount == null),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(refundPaymentResult);

        var refundEscrowResult = Result<EscrowOperationResponseDto>.Success(new EscrowOperationResponseDto
        {
            Message = "Escrow refunded",
            EscrowId = escrow.Id,
            RequestId = seed.ClientRequest.Id,
            Amount = 150m,
            Status = EscrowStatus.Refunded
        });

        mediatorMock.Setup(m => m.Send(
            It.Is<RefundEscrowCommand>(cmd => cmd.EscrowId == escrow.Id),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(refundEscrowResult);

        var handler = new CancelCaseCommandHandler(
            scope.DbContext,
            new FixedCurrentUserService(seed.Lawyer.Id),
            Mock.Of<INotificationService>(),
            mediatorMock.Object,
            Mock.Of<ILogger<CancelCaseCommandHandler>>());

        var cancelResult = await handler.Handle(
            new CancelCaseCommand(seed.Case.Id, "Lawyer cancels"),
            CancellationToken.None);

        Assert.True(cancelResult.IsSuccess);
        Assert.Equal(CaseStatus.Canceled, seed.Case.Status);
        Assert.Equal("Lawyer cancels", seed.Case.CancellationReason);
        Assert.Equal(ClientRequestStatus.Cancelled, seed.ClientRequest.Status);

        mediatorMock.Verify(m => m.Send(
            It.Is<RefundPaymentCommand>(cmd => cmd.PaymentId == payment.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        mediatorMock.Verify(m => m.Send(
            It.Is<RefundEscrowCommand>(cmd => cmd.EscrowId == escrow.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClientConfirmsPendingCase_ClosesCaseAndCompletesRequest()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedPlatformCaseAsync(scope.DbContext, "202", CaseStatus.PendingConfirmation);

        // Seed the Invoice
        var invoice = new Invoice
        {
            ReferenceNumber = $"INV-CONF-202",
            PlatformFee = 15m,
            LawyerAmount = 135m,
            TotalAmount = 150m,
            Status = InvoiceStatus.Paid,
            ClientRequestId = seed.ClientRequest.Id
        };
        scope.DbContext.Invoices.Add(invoice);

        // Seed the Escrow
        var escrow = new Escrow
        {
            RequestId = seed.ClientRequest.Id,
            Amount = 150m,
            Status = EscrowStatus.Funded,
            EscrowFundedAt = DateTime.UtcNow
        };
        scope.DbContext.Escrows.Add(escrow);
        await scope.DbContext.SaveChangesAsync();

        var handler = new ConfirmCaseCompletionCommandHandler(
            scope.DbContext,
            new FixedCurrentUserService(seed.Client.Id),
            Mock.Of<INotificationService>(),
            new EscrowSettlementService(scope.DbContext, NullLogger<EscrowSettlementService>.Instance),
            Mock.Of<ILogger<ConfirmCaseCompletionCommandHandler>>());

        var result = await handler.Handle(
            new ConfirmCaseCompletionCommand(seed.Case.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var secondResult = await handler.Handle(
            new ConfirmCaseCompletionCommand(seed.Case.Id),
            CancellationToken.None);

        Assert.False(secondResult.IsSuccess);
        Assert.Equal(CaseStatus.Closed, seed.Case.Status);
        Assert.Equal(ClientRequestStatus.Completed, seed.ClientRequest.Status);

        // Assert escrow status is Released
        Assert.Equal(EscrowStatus.Released, escrow.Status);
        Assert.NotNull(escrow.EscrowReleasedAt);

        // Assert lawyer wallet has been credited
        var wallet = await scope.DbContext.Wallets.FirstOrDefaultAsync(w => w.LawyerId == seed.Lawyer.Id);
        Assert.NotNull(wallet);
        Assert.Equal(135m, wallet.AvailableBalance);
        Assert.Equal(135m, wallet.Total);

        // Assert wallet transaction is created
        var tx = await scope.DbContext.WalletTransactions.FirstOrDefaultAsync(w => w.EscrowId == escrow.Id);
        Assert.NotNull(tx);
        Assert.Equal(135m, tx.Amount);
        Assert.Equal(TransactionType.Credit, tx.Type);
        Assert.Equal(1, await scope.DbContext.WalletTransactions.CountAsync(w => w.EscrowId == escrow.Id));
    }

    [Fact]
    public async Task GetCaseById_ReturnsActionIdsForFeedbackReportAndChat()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedPlatformCaseAsync(scope.DbContext, "203", CaseStatus.PendingConfirmation);

        var handler = new GetCaseByIdQueryHandler(
            scope.DbContext,
            new FixedCurrentUserService(seed.Client.Id));

        var result = await handler.Handle(new GetCaseByIdQuery(seed.Case.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(seed.ClientRequest.Id, result.Value!.ClientRequestId);
        Assert.Equal(seed.ClientRequest.RequestId, result.Value.ClientRequestReference);
        Assert.Equal(seed.Lawyer.Id, result.Value.LawyerId);
        Assert.Equal(seed.Service.Id, result.Value.ServiceId);
        Assert.Equal(seed.Chat.Id, result.Value.ChatId);
    }

    private static async Task<SeededPlatformCase> SeedPlatformCaseAsync(
        AlMostashar.Infrastructure.Data.AlmostasharDbContext db,
        string suffix,
        CaseStatus caseStatus)
    {
        var admin = TestEntityFactory.CreateAdmin(suffix);
        var client = TestEntityFactory.CreateClient(suffix);
        var lawyer = TestEntityFactory.CreateLawyer(suffix);
        db.AddRange(admin, client, lawyer);
        await db.SaveChangesAsync();

        var service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, suffix);
        db.LegalServices.Add(service);
        await db.SaveChangesAsync();

        db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer.Id, service.Id));
        await db.SaveChangesAsync();

        var clientRequest = new DirectRequest
        {
            RequestId = $"REQ-{suffix}",
            Title = "Need consultation",
            ProblemDetails = "details",
            ClientId = client.Id,
            LawyerServiceLawyerId = lawyer.Id,
            LawyerServiceLegalServiceId = service.Id,
            Status = ClientRequestStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        var caseEntity = CaseFactory.Create(ServiceType.Consultation, lawyer.Id, "Consultation case");
        caseEntity.Status = caseStatus;
        caseEntity.CaseClientRequest = new CaseClientRequest
        {
            Case = caseEntity,
            ClientRequest = clientRequest,
            CreatedAt = DateTime.UtcNow
        };

        db.Cases.Add(caseEntity);
        await db.SaveChangesAsync();

        var chat = Chat.Create(caseEntity.Id, lawyer.Id, client.Id);
        db.Chats.Add(chat);
        await db.SaveChangesAsync();

        return new SeededPlatformCase(admin, client, lawyer, service, clientRequest, caseEntity, chat);
    }

    private sealed record SeededPlatformCase(
        Admin Admin,
        Client Client,
        Lawyer Lawyer,
        LegalService Service,
        ClientRequest ClientRequest,
        Case Case,
        Chat Chat);
}
