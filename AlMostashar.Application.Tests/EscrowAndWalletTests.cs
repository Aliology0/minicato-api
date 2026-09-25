using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Admin.Commands.VerifyLawyer;
using AlMostashar.Application.Features.Escrows.Commands.MarkEscrowAsDisputed;
using AlMostashar.Application.Features.Escrows.Commands.RefundEscrow;
using AlMostashar.Application.Features.Escrows.Commands.ReleaseEscrow;
using AlMostashar.Application.Features.Escrows.EventHandlers;
using AlMostashar.Application.Features.Escrows.Queries.GetRequestEscrow;
using AlMostashar.Application.Features.Escrows.Services;
using AlMostashar.Application.Features.Wallets.Queries.GetMyWallet;
using AlMostashar.Application.Features.Wallets.Queries.GetMyWalletTransactions;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using AlMostashar.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AlMostashar.Application.Tests;

public class EscrowAndWalletTests
{
    // ──────────────────────────────────────────────────
    // Test 1: InvoicePaidEvent creates Funded escrow
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task InvoicePaidEvent_CreatesFundedEscrow_WithPaymentId_WithoutWallet()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var seed = await SeedPaidRequestWithPaymentAsync(db, "101");

        var handler = CreateInvoicePaidEventHandler(db);
        await handler.Handle(CreateInvoicePaidEvent(seed), CancellationToken.None);

        var escrow = await db.Escrows.SingleAsync(e => e.RequestId == seed.Request.Id);
        Assert.Equal(EscrowStatus.Funded, escrow.Status);
        Assert.Equal(seed.Invoice.TotalAmount, escrow.Amount);
        Assert.Equal(seed.Payment.Id, escrow.PaymentId);
        Assert.NotNull(escrow.EscrowFundedAt);

        Assert.Equal(0, await db.Wallets.CountAsync());
        Assert.Equal(0, await db.WalletTransactions.CountAsync());
    }

    // ──────────────────────────────────────────────────
    // Test 2: Duplicate InvoicePaidEvent — idempotent
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task InvoicePaidEvent_WhenRepublished_DoesNotDuplicateEscrow()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var seed = await SeedPaidRequestWithPaymentAsync(db, "102");

        var handler = CreateInvoicePaidEventHandler(db);
        var notification = CreateInvoicePaidEvent(seed);

        await handler.Handle(notification, CancellationToken.None);
        await handler.Handle(notification, CancellationToken.None);

        Assert.Equal(1, await db.Escrows.CountAsync(e => e.RequestId == seed.Request.Id));
        Assert.Equal(0, await db.WalletTransactions.CountAsync());
    }

    // ──────────────────────────────────────────────────
    // Test 3: Existing NotFunded escrow becomes Funded
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task FundEscrow_ExistingNotFunded_BecomesFunded()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var seed = await SeedPaidRequestWithPaymentAsync(db, "103");

        db.Escrows.Add(new Escrow
        {
            RequestId = seed.Request.Id,
            Amount = 0,
            Status = EscrowStatus.NotFunded
        });
        await db.SaveChangesAsync();

        var handler = CreateInvoicePaidEventHandler(db);
        await handler.Handle(CreateInvoicePaidEvent(seed), CancellationToken.None);

        var escrow = await db.Escrows.SingleAsync(e => e.RequestId == seed.Request.Id);
        Assert.Equal(EscrowStatus.Funded, escrow.Status);
        Assert.Equal(seed.Payment.Id, escrow.PaymentId);
        Assert.Equal(seed.Payment.Amount, escrow.Amount);
        Assert.NotNull(escrow.EscrowFundedAt);
    }

    // ──────────────────────────────────────────────────
    // Test 4: Existing Funded same amount/payment — no-op
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task FundEscrow_ExistingFundedSameAmountPayment_IsIdempotent()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var seed = await SeedPaidRequestWithPaymentAsync(db, "104");

        db.Escrows.Add(new Escrow
        {
            RequestId = seed.Request.Id,
            PaymentId = seed.Payment.Id,
            Amount = seed.Payment.Amount,
            Status = EscrowStatus.Funded,
            EscrowFundedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateEscrowFundingService(db);
        await svc.FundEscrowAsync(seed.Request.Id, seed.Payment.Id, seed.Payment.Amount, CancellationToken.None);

        Assert.Equal(1, await db.Escrows.CountAsync());
    }

    // ──────────────────────────────────────────────────
    // Test 5: Existing Funded different amount — no mutation
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task FundEscrow_ExistingFundedDifferentAmount_NoMutation()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var seed = await SeedPaidRequestWithPaymentAsync(db, "105");

        db.Escrows.Add(new Escrow
        {
            RequestId = seed.Request.Id,
            PaymentId = seed.Payment.Id,
            Amount = 999m,
            Status = EscrowStatus.Funded,
            EscrowFundedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateEscrowFundingService(db);
        await svc.FundEscrowAsync(seed.Request.Id, seed.Payment.Id, seed.Payment.Amount, CancellationToken.None);

        var escrow = await db.Escrows.SingleAsync();
        Assert.Equal(999m, escrow.Amount); // unchanged
    }

    // ──────────────────────────────────────────────────
    // Test 6: Existing Funded different PaymentId — no mutation
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task FundEscrow_ExistingFundedDifferentPayment_NoMutation()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var seed = await SeedPaidRequestWithPaymentAsync(db, "106");

        // Create a second payment to use as the "different" payment
        var otherPayment = new Payment
        {
            InvoiceId = seed.Invoice.Id,
            TransactionId = 888888,
            Status = PaymentStatus.Succeeded,
            Amount = seed.Payment.Amount,
            PayDate = DateTime.UtcNow,
            PaymentMethod = "CreditCard",
            PaymentProvider = PaymentProvider.Paymob,
            GatewayResponse = "{}"
        };
        db.Payments.Add(otherPayment);
        await db.SaveChangesAsync();

        db.Escrows.Add(new Escrow
        {
            RequestId = seed.Request.Id,
            PaymentId = otherPayment.Id,
            Amount = seed.Payment.Amount,
            Status = EscrowStatus.Funded,
            EscrowFundedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateEscrowFundingService(db);
        await svc.FundEscrowAsync(seed.Request.Id, seed.Payment.Id, seed.Payment.Amount, CancellationToken.None);

        var escrow = await db.Escrows.SingleAsync();
        Assert.Equal(otherPayment.Id, escrow.PaymentId); // unchanged
    }

    // ──────────────────────────────────────────────────
    // Test 7: Released/Refunded/Disputed escrow — not modified
    // ──────────────────────────────────────────────────
    [Theory]
    [InlineData(EscrowStatus.Released)]
    [InlineData(EscrowStatus.Refunded)]
    [InlineData(EscrowStatus.Disputed)]
    public async Task FundEscrow_TerminalStatus_NoMutation(EscrowStatus status)
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var seed = await SeedPaidRequestWithPaymentAsync(db, $"107-{status}");

        db.Escrows.Add(new Escrow
        {
            RequestId = seed.Request.Id,
            Amount = seed.Payment.Amount,
            Status = status,
            EscrowFundedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = CreateEscrowFundingService(db);
        await svc.FundEscrowAsync(seed.Request.Id, seed.Payment.Id, seed.Payment.Amount, CancellationToken.None);

        var escrow = await db.Escrows.SingleAsync();
        Assert.Equal(status, escrow.Status);
    }

    // ──────────────────────────────────────────────────
    // Test 8: ReleaseEscrow credits wallet with LawyerAmount
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task ReleaseEscrow_CreditsInvoiceLawyerAmount_AndSetsEscrowId()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var escrow = await SeedFundedEscrowAsync(db, "108");
        var invoice = await db.Invoices.SingleAsync(i => i.ClientRequestId == escrow.RequestId);

        var handler = CreateReleaseEscrowCommandHandler(db);
        var result = await handler.Handle(new ReleaseEscrowCommand(escrow.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(invoice.LawyerAmount, result.Value!.ReleasedAmount);

        var refreshedEscrow = await db.Escrows.SingleAsync(e => e.Id == escrow.Id);
        Assert.Equal(EscrowStatus.Released, refreshedEscrow.Status);
        Assert.NotNull(refreshedEscrow.EscrowReleasedAt);

        var wallet = await db.Wallets
            .Include(w => w.WalletTransactions)
            .SingleAsync(w => w.LawyerId == escrow.Request.LawyerServiceLawyerId);

        Assert.Equal(invoice.LawyerAmount, wallet.AvailableBalance);

        var transaction = Assert.Single(wallet.WalletTransactions);
        Assert.Equal(invoice.LawyerAmount, transaction.Amount);
        Assert.Equal(TransactionType.Credit, transaction.Type);
        Assert.Equal("Escrow", transaction.ReferenceType);
        Assert.Equal(escrow.Id.ToString(), transaction.ReferenceId);
        Assert.Equal(escrow.Id, transaction.EscrowId);
        Assert.Equal(invoice.Id, transaction.InvoiceId);
        Assert.Equal(wallet.AvailableBalance, transaction.BalanceAfter);
        Assert.NotEqual(default, transaction.CreatedAt);

        var paymentLink = await db.PaymentWalletTransactions.SingleAsync();
        Assert.Equal(escrow.PaymentId, paymentLink.PaymentId);
        Assert.Equal(transaction.Id, paymentLink.WalletTransactionId);
    }

    // ──────────────────────────────────────────────────
    // Test 8b: ReleaseEscrow does not double credit
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task ReleaseEscrow_WhenAlreadyReleased_DoesNotCreditWalletAgain()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var escrow = await SeedFundedEscrowAsync(db, "108b");

        var handler = CreateReleaseEscrowCommandHandler(db);
        var first = await handler.Handle(new ReleaseEscrowCommand(escrow.Id), CancellationToken.None);
        var second = await handler.Handle(new ReleaseEscrowCommand(escrow.Id), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal(1, await db.WalletTransactions.CountAsync());
    }

    // ──────────────────────────────────────────────────
    // Test 9: Refund before release — wallet unchanged
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task RefundEscrow_BeforeRelease_WalletUnchanged()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var escrow = await SeedFundedEscrowAsync(db, "109");

        var result = await new RefundEscrowCommandHandler(db)
            .Handle(new RefundEscrowCommand(escrow.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var refreshed = await db.Escrows.SingleAsync(e => e.Id == escrow.Id);
        Assert.Equal(EscrowStatus.Refunded, refreshed.Status);
        Assert.Equal(0, await db.WalletTransactions.CountAsync());
        Assert.Equal(0, await db.Wallets.CountAsync());
    }

    // ──────────────────────────────────────────────────
    // Test 10: Disputed escrow blocks normal release
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task MarkEscrowAsDisputed_BlocksNormalRelease()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var escrow = await SeedFundedEscrowAsync(db, "110");

        var disputeResult = await new MarkEscrowAsDisputedCommandHandler(db)
            .Handle(new MarkEscrowAsDisputedCommand(escrow.Id), CancellationToken.None);

        var releaseResult = await CreateReleaseEscrowCommandHandler(db)
            .Handle(new ReleaseEscrowCommand(escrow.Id), CancellationToken.None);

        Assert.True(disputeResult.IsSuccess);
        Assert.False(releaseResult.IsSuccess);

        var refreshed = await db.Escrows.SingleAsync(e => e.Id == escrow.Id);
        Assert.Equal(EscrowStatus.Disputed, refreshed.Status);
        Assert.NotNull(refreshed.EscrowDisputedAt);
        Assert.Equal(0, await db.WalletTransactions.CountAsync());
    }

    // ──────────────────────────────────────────────────
    // Test: Refund blocks released escrow
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task RefundEscrow_BlocksReleasedEscrow()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var escrow = await SeedFundedEscrowAsync(db, "111");

        await CreateReleaseEscrowCommandHandler(db)
            .Handle(new ReleaseEscrowCommand(escrow.Id), CancellationToken.None);

        var result = await new RefundEscrowCommandHandler(db)
            .Handle(new RefundEscrowCommand(escrow.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Escrow.InvalidStatus", result.Error?.Code);
    }

    // ──────────────────────────────────────────────────
    // Test: Release blocks refunded/disputed
    // ──────────────────────────────────────────────────
    [Theory]
    [InlineData(EscrowStatus.Refunded)]
    [InlineData(EscrowStatus.Disputed)]
    public async Task ReleaseEscrow_BlocksRefundedAndDisputedEscrows(EscrowStatus status)
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var escrow = await SeedFundedEscrowAsync(db, $"112-{status}");
        escrow.Status = status;
        await db.SaveChangesAsync();

        var handler = CreateReleaseEscrowCommandHandler(db);
        var result = await handler.Handle(new ReleaseEscrowCommand(escrow.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Escrow.InvalidStatus", result.Error?.Code);
        Assert.Equal(0, await db.WalletTransactions.CountAsync());
    }

    // ──────────────────────────────────────────────────
    // Test 15: GetMyWallet uses decimal escrow balance
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task GetMyWallet_ReturnsCorrectFundedEscrowBalance()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var funded = await SeedFundedEscrowAsync(db, "113", 300m);
        var released = await SeedFundedEscrowAsync(db, "114", 75m, funded.Request.LawyerServiceLawyerId);
        released.Status = EscrowStatus.Released;
        await SeedFundedEscrowAsync(db, "115", 500m); // other lawyer

        db.Wallets.Add(new Wallet
        {
            LawyerId = funded.Request.LawyerServiceLawyerId!.Value,
            AvailableBalance = 120m,
            Total = 420m
        });
        await db.SaveChangesAsync();

        var handler = new GetMyWalletQueryHandler(
            db, new FixedCurrentUserService(funded.Request.LawyerServiceLawyerId!.Value));

        var result = await handler.Handle(new GetMyWalletQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(120m, result.Value!.AvailableBalance);
        Assert.Equal(420m, result.Value.TotalEarnings);
        Assert.Equal(300m, result.Value.EscrowBalance);
        Assert.Equal(0m, result.Value.PendingBalance);
    }

    // ──────────────────────────────────────────────────
    // Test: VerifyLawyer creates zero-balance wallet
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task VerifyLawyer_WhenAccepted_CreatesZeroBalanceWallet()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("116");
        var lawyer = TestEntityFactory.CreateLawyer("116", isVerified: false);
        db.AddRange(admin, lawyer);
        await db.SaveChangesAsync();

        var handler = new VerifyLawyerCommandHandler(db, new NoOpEmailService(), new FixedCurrentUserService(admin.Id), Microsoft.Extensions.Logging.Abstractions.NullLogger<VerifyLawyerCommandHandler>.Instance);
        var result = await handler.Handle(
            new VerifyLawyerCommand { LawyerId = lawyer.Id, IsAccepted = true },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var wallet = await db.Wallets.SingleAsync(w => w.LawyerId == lawyer.Id);
        Assert.Equal(0m, wallet.AvailableBalance);
    }

    // ──────────────────────────────────────────────────
    // Test: GetRequestEscrow access control
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task GetRequestEscrow_AllowsOwnerClientAssignedLawyerAndAdmin()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var escrow = await SeedFundedEscrowAsync(db, "117");
        var request = await db.ClientRequests.AsNoTracking().SingleAsync(r => r.Id == escrow.RequestId);

        var admin = TestEntityFactory.CreateAdmin("117");
        db.Admins.Add(admin);
        await db.SaveChangesAsync();

        var clientResult = await new GetRequestEscrowQueryHandler(db, new FixedCurrentUserService(request.ClientId))
            .Handle(new GetRequestEscrowQuery(request.Id), CancellationToken.None);
        var lawyerResult = await new GetRequestEscrowQueryHandler(db, new FixedCurrentUserService(request.LawyerServiceLawyerId!.Value))
            .Handle(new GetRequestEscrowQuery(request.Id), CancellationToken.None);
        var adminResult = await new GetRequestEscrowQueryHandler(db, new FixedCurrentUserService(admin.Id))
            .Handle(new GetRequestEscrowQuery(request.Id), CancellationToken.None);

        Assert.True(clientResult.IsSuccess);
        Assert.True(lawyerResult.IsSuccess);
        Assert.True(adminResult.IsSuccess);
    }

    [Fact]
    public async Task GetRequestEscrow_DeniesUnrelatedUser()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var escrow = await SeedFundedEscrowAsync(db, "118");
        var unrelated = TestEntityFactory.CreateClient("118");
        db.Clients.Add(unrelated);
        await db.SaveChangesAsync();

        var result = await new GetRequestEscrowQueryHandler(db, new FixedCurrentUserService(unrelated.Id))
            .Handle(new GetRequestEscrowQuery(escrow.RequestId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Escrow.Auth.Unauthorized", result.Error?.Code);
    }

    // ──────────────────────────────────────────────────
    // Test: Wallet transactions query
    // ──────────────────────────────────────────────────
    [Fact]
    public async Task GetMyWalletTransactions_ReturnsLatestTransactionsWithCursor()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var escrow = await SeedFundedEscrowAsync(db, "119");
        var lawyerId = escrow.Request.LawyerServiceLawyerId!.Value;

        var wallet = new Wallet { LawyerId = lawyerId, AvailableBalance = 30m, Total = 30m };
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();

        db.WalletTransactions.AddRange(
            new WalletTransaction { WalletId = wallet.Id, Amount = 10m, Type = TransactionType.Credit, ReferenceType = "Manual", ReferenceId = "1" },
            new WalletTransaction { WalletId = wallet.Id, Amount = 20m, Type = TransactionType.Credit, ReferenceType = "Escrow", ReferenceId = escrow.Id.ToString(), EscrowId = escrow.Id });
        await db.SaveChangesAsync();

        var handler = new GetMyWalletTransactionsQueryHandler(db, new FixedCurrentUserService(lawyerId));
        var firstPage = await handler.Handle(new GetMyWalletTransactionsQuery(PageSize: 1), CancellationToken.None);

        Assert.True(firstPage.IsSuccess);
        Assert.Single(firstPage.Value!.Items);
        Assert.True(firstPage.Value.HasMore);
        Assert.NotEqual(default, firstPage.Value.Items[0].CreatedAt);
        Assert.NotNull(firstPage.Value.Items[0].ReferenceType);
    }

    // ══════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════

    private static EscrowFundingService CreateEscrowFundingService(AlmostasharDbContext db)
    {
        return new EscrowFundingService(db, NullLogger<EscrowFundingService>.Instance);
    }

    private static EscrowSettlementService CreateEscrowSettlementService(AlmostasharDbContext db)
    {
        return new EscrowSettlementService(db, NullLogger<EscrowSettlementService>.Instance);
    }

    private static ReleaseEscrowCommandHandler CreateReleaseEscrowCommandHandler(AlmostasharDbContext db)
    {
        return new ReleaseEscrowCommandHandler(db, CreateEscrowSettlementService(db));
    }

    private static FundEscrowOnInvoicePaidEventHandler CreateInvoicePaidEventHandler(AlmostasharDbContext db)
    {
        var svc = CreateEscrowFundingService(db);
        return new FundEscrowOnInvoicePaidEventHandler(svc, NullLogger<FundEscrowOnInvoicePaidEventHandler>.Instance);
    }

    private static InvoicePaidEvent CreateInvoicePaidEvent(PaidRequestSeed seed)
    {
        return new InvoicePaidEvent(
            seed.Invoice.Id,
            seed.Request.Id,
            seed.Payment.Id,
            seed.Payment.TransactionId!.Value,
            paymobOrderId: 123,
            paidAmount: seed.Payment.Amount);
    }

    private static async Task<Escrow> SeedFundedEscrowAsync(
        AlmostasharDbContext db, string suffix, decimal amount = 300m, int? existingLawyerId = null)
    {
        var seed = await SeedPaidRequestWithPaymentAsync(db, suffix, amount, existingLawyerId);
        var escrow = new Escrow
        {
            RequestId = seed.Request.Id,
            Request = seed.Request,
            PaymentId = seed.Payment.Id,
            Amount = amount,
            Status = EscrowStatus.Funded,
            EscrowFundedAt = DateTime.UtcNow
        };
        db.Escrows.Add(escrow);
        await db.SaveChangesAsync();
        return escrow;
    }

    private static async Task<PaidRequestSeed> SeedPaidRequestWithPaymentAsync(
        AlmostasharDbContext db, string suffix, decimal amount = 300m, int? existingLawyerId = null)
    {
        var admin = TestEntityFactory.CreateAdmin($"esc-{suffix}");
        var client = TestEntityFactory.CreateClient($"esc-{suffix}");
        Lawyer lawyer;

        if (existingLawyerId.HasValue)
        {
            lawyer = await db.Lawyers.SingleAsync(l => l.Id == existingLawyerId.Value);
            db.AddRange(admin, client);
        }
        else
        {
            lawyer = TestEntityFactory.CreateLawyer(NumericSuffix(suffix));
            db.AddRange(admin, client, lawyer);
        }
        await db.SaveChangesAsync();

        var service = await db.LegalServices.FirstOrDefaultAsync(ls => ls.ServiceType == ServiceType.Consultation);
        if (service is null)
        {
            service = TestEntityFactory.CreateLegalService(admin.Id, ServiceType.Consultation, $"esc-{suffix}");
            db.LegalServices.Add(service);
            await db.SaveChangesAsync();
        }

        if (!await db.LawyerServices.AnyAsync(ls => ls.LawyerId == lawyer.Id && ls.LegalServiceId == service.Id))
        {
            db.LawyerServices.Add(TestEntityFactory.CreateLawyerService(lawyer.Id, service.Id, amount));
            await db.SaveChangesAsync();
        }

        var request = new DirectRequest
        {
            RequestId = $"REQ-ESC-{suffix}",
            Title = $"Escrow Request {suffix}",
            ProblemDetails = "details",
            Governorate = "Cairo",
            ClientId = client.Id,
            Status = ClientRequestStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            LawyerServiceLawyerId = lawyer.Id,
            LawyerServiceLegalServiceId = service.Id
        };
        db.DirectRequests.Add(request);
        await db.SaveChangesAsync();

        var invoice = new Invoice
        {
            ReferenceNumber = $"INV-ESC-{suffix}",
            PlatformFee = Math.Round(amount * 0.10m, 2),
            LawyerAmount = amount - Math.Round(amount * 0.10m, 2),
            TotalAmount = amount,
            Status = InvoiceStatus.Paid,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            PaidAt = DateTime.UtcNow,
            ClientRequestId = request.Id
        };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            TransactionId = 900000 + int.Parse(NumericSuffix(suffix)),
            Status = PaymentStatus.Succeeded,
            Amount = amount,
            PayDate = DateTime.UtcNow,
            PaymentMethod = "CreditCard",
            PaymentProvider = PaymentProvider.Paymob,
            GatewayResponse = "{}"
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        return new PaidRequestSeed(client, lawyer, request, invoice, payment);
    }

    private sealed record PaidRequestSeed(Client Client, Lawyer Lawyer, ClientRequest Request, Invoice Invoice, Payment Payment);

    private static string NumericSuffix(string suffix)
    {
        var digits = new string(suffix.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? "1" : digits;
    }
}
