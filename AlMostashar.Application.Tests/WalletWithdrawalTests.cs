using AlMostashar.Application.Features.Admin.Commands.VerifyLawyer;
using AlMostashar.Application.Features.Wallets.Commands.ApproveWithdrawal;
using AlMostashar.Application.Features.Wallets.Commands.CancelWithdrawal;
using AlMostashar.Application.Features.Wallets.Commands.MarkWithdrawalPaid;
using AlMostashar.Application.Features.Wallets.Commands.RejectWithdrawal;
using AlMostashar.Application.Features.Wallets.Commands.RequestWithdrawal;
using AlMostashar.Application.Features.Wallets.Queries.GetAdminWithdrawalById;
using AlMostashar.Application.Features.Wallets.Queries.GetAdminWithdrawals;
using AlMostashar.Application.Features.Wallets.Queries.GetMyWallet;
using AlMostashar.Application.Features.Wallets.Queries.GetMyWalletTransactions;
using AlMostashar.Application.Features.Wallets.Queries.GetMyWithdrawals;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using AlMostashar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AlMostashar.Application.Tests;

public class WalletWithdrawalTests
{
    [Fact]
    public async Task NewVerifiedLawyer_HasZeroWalletSummary()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var admin = TestEntityFactory.CreateAdmin("ww-001");
        var lawyer = TestEntityFactory.CreateLawyer("1001", isVerified: false);
        db.AddRange(admin, lawyer);
        await db.SaveChangesAsync();

        var verify = await new VerifyLawyerCommandHandler(
                db,
                new NoOpEmailService(),
                new FixedCurrentUserService(admin.Id),
                NullLogger<VerifyLawyerCommandHandler>.Instance)
            .Handle(new VerifyLawyerCommand { LawyerId = lawyer.Id, IsAccepted = true }, CancellationToken.None);

        Assert.True(verify.IsSuccess);

        var summary = await new GetMyWalletQueryHandler(db, new FixedCurrentUserService(lawyer.Id))
            .Handle(new GetMyWalletQuery(), CancellationToken.None);

        Assert.True(summary.IsSuccess);
        Assert.Equal(0m, summary.Value!.AvailableBalance);
        Assert.Equal(0m, summary.Value.TotalEarnings);
        Assert.Equal(0m, summary.Value.EscrowBalance);
        Assert.Equal(0m, summary.Value.PendingWithdrawalBalance);
    }

    [Fact]
    public async Task LawyerCanRequestWithdrawalWithinAvailableBalance_AndAmountIsReserved()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1002", 500m);

        var result = await CreateRequestWithdrawalHandler(scope.DbContext, seed.Lawyer.Id)
            .Handle(new RequestWithdrawalCommand
            {
                Amount = 150m,
                Method = WithdrawalMethod.InstaPay,
                AccountDetails = "instapay@example"
            }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(WithdrawalStatus.Pending, result.Value!.Status);
        Assert.Equal("********mple", result.Value.AccountDetails);
        Assert.Equal("********mple", result.Value.AccountDetailsMasked);
        Assert.Null(result.Value.AccountDetailsFull);

        var wallet = await scope.DbContext.Wallets.SingleAsync(w => w.Id == seed.Wallet.Id);
        Assert.Equal(350m, wallet.AvailableBalance);

        var storedWithdrawal = await scope.DbContext.WithdrawalRequests.SingleAsync(wr => wr.Id == result.Value.Id);
        Assert.Equal("********mple", storedWithdrawal.AccountDetailsMasked);
        Assert.NotEqual("instapay@example", storedWithdrawal.AccountDetailsEncrypted);

        var transaction = await scope.DbContext.WalletTransactions.SingleAsync();
        Assert.Equal(TransactionType.Debit, transaction.Type);
        Assert.Equal("Withdrawal", transaction.ReferenceType);
        Assert.Equal(result.Value.Id, transaction.WithdrawalRequestId);
        Assert.Equal(350m, transaction.BalanceAfter);
        Assert.NotEqual(default, transaction.CreatedAt);
    }

    [Fact]
    public async Task LawyerCannotRequestWithdrawalAboveAvailableBalance()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1003", 50m);

        var result = await CreateRequestWithdrawalHandler(scope.DbContext, seed.Lawyer.Id)
            .Handle(new RequestWithdrawalCommand
            {
                Amount = 75m,
                Method = WithdrawalMethod.BankTransfer,
                AccountDetails = "Bank account"
            }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Withdrawal.InsufficientBalance", result.Error?.Code);
        Assert.Equal(50m, (await scope.DbContext.Wallets.SingleAsync(w => w.Id == seed.Wallet.Id)).AvailableBalance);
        Assert.Equal(0, await scope.DbContext.WithdrawalRequests.CountAsync());
        Assert.Equal(0, await scope.DbContext.WalletTransactions.CountAsync());
    }

    [Fact]
    public async Task PendingWithdrawalAppearsInWalletSummary()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1004", 400m);

        await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 125m);

        var summary = await new GetMyWalletQueryHandler(scope.DbContext, new FixedCurrentUserService(seed.Lawyer.Id))
            .Handle(new GetMyWalletQuery(), CancellationToken.None);

        Assert.True(summary.IsSuccess);
        Assert.Equal(275m, summary.Value!.AvailableBalance);
        Assert.Equal(125m, summary.Value.PendingWithdrawalBalance);
        Assert.Equal(125m, summary.Value.PendingBalance);
    }

    [Fact]
    public async Task LawyerCanListOnlyOwnWithdrawals()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var first = await SeedLawyerWalletAsync(scope.DbContext, "1005", 300m);
        var second = await SeedLawyerWalletAsync(scope.DbContext, "1006", 300m);

        await RequestWithdrawalAsync(scope.DbContext, first.Lawyer.Id, 100m);
        await RequestWithdrawalAsync(scope.DbContext, second.Lawyer.Id, 120m);

        var result = await new GetMyWithdrawalsQueryHandler(scope.DbContext, new FixedCurrentUserService(first.Lawyer.Id))
            .Handle(new GetMyWithdrawalsQuery(PageSize: 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(first.Lawyer.Id, item.LawyerId);
        Assert.Equal(100m, item.Amount);
        Assert.Equal("*******0000", item.AccountDetails);
        Assert.Null(item.AccountDetailsFull);
    }

    [Fact]
    public async Task LawyerCanCancelPendingWithdrawal_AndAmountReturnsToAvailableBalance()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1007", 250m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 80m);

        var result = await new CancelWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Lawyer.Id))
            .Handle(new CancelWithdrawalCommand(withdrawal.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(WithdrawalStatus.Cancelled, result.Value!.Status);

        var wallet = await scope.DbContext.Wallets.SingleAsync(w => w.Id == seed.Wallet.Id);
        Assert.Equal(250m, wallet.AvailableBalance);

        Assert.Equal(2, await scope.DbContext.WalletTransactions.CountAsync(t => t.WithdrawalRequestId == withdrawal.Id));
        Assert.Contains(await scope.DbContext.WalletTransactions.ToListAsync(), t => t.ReferenceType == "WithdrawalCancellation");
    }

    [Fact]
    public async Task LawyerCannotCancelAnotherLawyersWithdrawal()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var first = await SeedLawyerWalletAsync(scope.DbContext, "1008", 250m);
        var second = await SeedLawyerWalletAsync(scope.DbContext, "1009", 250m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, first.Lawyer.Id, 80m);

        var result = await new CancelWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(second.Lawyer.Id))
            .Handle(new CancelWithdrawalCommand(withdrawal.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Withdrawal.NotFound", result.Error?.Code);
    }

    [Fact]
    public async Task AdminCanApproveWithdrawal()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1010", 250m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 80m);

        var result = await new ApproveWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new ApproveWithdrawalCommand
            {
                WithdrawalId = withdrawal.Id,
                AdminNotes = "Approved for manual payout"
            }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(WithdrawalStatus.Approved, result.Value!.Status);
        Assert.Equal(seed.Admin.Id, result.Value.ReviewedByAdminId);
        Assert.Equal(170m, (await scope.DbContext.Wallets.SingleAsync(w => w.Id == seed.Wallet.Id)).AvailableBalance);
    }

    [Fact]
    public async Task AdminCanRejectPendingOrApprovedWithdrawal_AndRestoreBalance()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1011", 250m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 80m);

        await new ApproveWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new ApproveWithdrawalCommand { WithdrawalId = withdrawal.Id }, CancellationToken.None);

        var result = await new RejectWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new RejectWithdrawalCommand
            {
                WithdrawalId = withdrawal.Id,
                RejectionReason = "Manual payout details are invalid"
            }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(WithdrawalStatus.Rejected, result.Value!.Status);
        Assert.Equal(250m, (await scope.DbContext.Wallets.SingleAsync(w => w.Id == seed.Wallet.Id)).AvailableBalance);
        Assert.Contains(await scope.DbContext.WalletTransactions.ToListAsync(), t => t.ReferenceType == "WithdrawalRejection");
    }

    [Fact]
    public async Task AdminCanMarkApprovedWithdrawalAsPaid_ButCannotMarkRejectedOrCancelledAsPaid()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var approvedSeed = await SeedLawyerWalletAsync(scope.DbContext, "1012", 250m);
        var rejectedSeed = await SeedLawyerWalletAsync(scope.DbContext, "1013", 250m);
        var cancelledSeed = await SeedLawyerWalletAsync(scope.DbContext, "1014", 250m);

        var approved = await RequestWithdrawalAsync(scope.DbContext, approvedSeed.Lawyer.Id, 80m);
        await new ApproveWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(approvedSeed.Admin.Id))
            .Handle(new ApproveWithdrawalCommand { WithdrawalId = approved.Id }, CancellationToken.None);

        var paid = await new MarkWithdrawalPaidCommandHandler(scope.DbContext, new FixedCurrentUserService(approvedSeed.Admin.Id))
            .Handle(new MarkWithdrawalPaidCommand
            {
                WithdrawalId = approved.Id,
                PayoutReference = "PAYOUT-1012"
            }, CancellationToken.None);

        Assert.True(paid.IsSuccess);
        Assert.Equal(WithdrawalStatus.Paid, paid.Value!.Status);
        Assert.NotNull(paid.Value.PaidAt);
        Assert.Equal("PAYOUT-1012", paid.Value.PayoutReference);
        Assert.Equal("Manual", paid.Value.PayoutProvider);
        Assert.Equal(approvedSeed.Admin.Id, paid.Value.PaidByAdminId);

        var rejected = await RequestWithdrawalAsync(scope.DbContext, rejectedSeed.Lawyer.Id, 70m);
        await new RejectWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(rejectedSeed.Admin.Id))
            .Handle(new RejectWithdrawalCommand { WithdrawalId = rejected.Id, RejectionReason = "No" }, CancellationToken.None);

        var rejectedPaid = await new MarkWithdrawalPaidCommandHandler(scope.DbContext, new FixedCurrentUserService(rejectedSeed.Admin.Id))
            .Handle(new MarkWithdrawalPaidCommand
            {
                WithdrawalId = rejected.Id,
                PayoutReference = "PAYOUT-REJECTED"
            }, CancellationToken.None);

        Assert.False(rejectedPaid.IsSuccess);

        var cancelled = await RequestWithdrawalAsync(scope.DbContext, cancelledSeed.Lawyer.Id, 60m);
        await new CancelWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(cancelledSeed.Lawyer.Id))
            .Handle(new CancelWithdrawalCommand(cancelled.Id), CancellationToken.None);

        var cancelledPaid = await new MarkWithdrawalPaidCommandHandler(scope.DbContext, new FixedCurrentUserService(cancelledSeed.Admin.Id))
            .Handle(new MarkWithdrawalPaidCommand
            {
                WithdrawalId = cancelled.Id,
                PayoutReference = "PAYOUT-CANCELLED"
            }, CancellationToken.None);

        Assert.False(cancelledPaid.IsSuccess);
    }

    [Fact]
    public async Task WalletTransactionsArePaginatedAndIncludeReferenceInfo()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1015", 300m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 50m);
        await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 40m);

        var result = await new GetMyWalletTransactionsQueryHandler(scope.DbContext, new FixedCurrentUserService(seed.Lawyer.Id))
            .Handle(new GetMyWalletTransactionsQuery(PageSize: 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.True(result.Value.HasMore);
        Assert.NotEqual(default, result.Value.Items[0].CreatedAt);
        Assert.Equal("Withdrawal", result.Value.Items[0].ReferenceType);
        Assert.NotNull(result.Value.Items[0].ReferenceId);
        Assert.NotNull(result.Value.Items[0].BalanceAfter);
        Assert.NotNull(result.Value.Items[0].WithdrawalRequestId);
        Assert.Contains(await scope.DbContext.WalletTransactions.ToListAsync(), t => t.WithdrawalRequestId == withdrawal.Id);
    }

    [Fact]
    public async Task AdminCanListWithdrawals()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1016", 300m);
        await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 50m);

        var result = await new GetAdminWithdrawalsQueryHandler(scope.DbContext)
            .Handle(new GetAdminWithdrawalsQuery(WithdrawalStatus.Pending), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(seed.Lawyer.Id, item.LawyerId);
        Assert.Equal(seed.Lawyer.Email, item.LawyerEmail);
    }

    [Fact]
    public async Task ConcurrentLikeWithdrawalRequests_CannotOverdrawWallet()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1017", 100m);

        await using var firstContext = scope.CreateNewContext();
        await using var secondContext = scope.CreateNewContext();

        var first = await CreateRequestWithdrawalHandler(firstContext, seed.Lawyer.Id)
            .Handle(new RequestWithdrawalCommand
            {
                Amount = 80m,
                Method = WithdrawalMethod.BankTransfer,
                AccountDetails = "bank account 1"
            }, CancellationToken.None);

        var second = await CreateRequestWithdrawalHandler(secondContext, seed.Lawyer.Id)
            .Handle(new RequestWithdrawalCommand
            {
                Amount = 80m,
                Method = WithdrawalMethod.BankTransfer,
                AccountDetails = "bank account 2"
            }, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal("Withdrawal.InsufficientBalance", second.Error?.Code);

        await scope.DbContext.Entry(seed.Wallet).ReloadAsync();
        Assert.Equal(20m, seed.Wallet.AvailableBalance);
        Assert.Equal(1, await scope.DbContext.WithdrawalRequests.CountAsync());
        Assert.Equal(1, await scope.DbContext.WalletTransactions.CountAsync(t => t.Type == TransactionType.Debit));
    }

    [Fact]
    public async Task DuplicateCancel_DoesNotRestoreBalanceTwice()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1018", 200m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 75m);

        var handler = new CancelWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Lawyer.Id));
        var first = await handler.Handle(new CancelWithdrawalCommand(withdrawal.Id), CancellationToken.None);
        var second = await handler.Handle(new CancelWithdrawalCommand(withdrawal.Id), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal("Withdrawal.InvalidStatus", second.Error?.Code);
        Assert.Equal(200m, (await scope.DbContext.Wallets.SingleAsync(w => w.Id == seed.Wallet.Id)).AvailableBalance);
        Assert.Equal(1, await scope.DbContext.WalletTransactions.CountAsync(t => t.ReferenceType == "WithdrawalCancellation"));
    }

    [Fact]
    public async Task DuplicateReject_DoesNotRestoreBalanceTwice()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1019", 200m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 75m);

        var handler = new RejectWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id));
        var first = await handler.Handle(new RejectWithdrawalCommand
        {
            WithdrawalId = withdrawal.Id,
            RejectionReason = "Invalid payout details"
        }, CancellationToken.None);
        var second = await handler.Handle(new RejectWithdrawalCommand
        {
            WithdrawalId = withdrawal.Id,
            RejectionReason = "Duplicate rejection"
        }, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal("Withdrawal.InvalidStatus", second.Error?.Code);
        Assert.Equal(200m, (await scope.DbContext.Wallets.SingleAsync(w => w.Id == seed.Wallet.Id)).AvailableBalance);
        Assert.Equal(1, await scope.DbContext.WalletTransactions.CountAsync(t => t.ReferenceType == "WithdrawalRejection"));
    }

    [Fact]
    public async Task CancelThenAdminReject_DoesNotRestoreBalanceTwice()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1020", 200m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 75m);

        var cancel = await new CancelWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Lawyer.Id))
            .Handle(new CancelWithdrawalCommand(withdrawal.Id), CancellationToken.None);

        var reject = await new RejectWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new RejectWithdrawalCommand
            {
                WithdrawalId = withdrawal.Id,
                RejectionReason = "Admin rejection after cancellation"
            }, CancellationToken.None);

        Assert.True(cancel.IsSuccess);
        Assert.False(reject.IsSuccess);
        Assert.Equal(200m, (await scope.DbContext.Wallets.SingleAsync(w => w.Id == seed.Wallet.Id)).AvailableBalance);
        Assert.Equal(1, await scope.DbContext.WalletTransactions.CountAsync(t =>
            t.ReferenceType == "WithdrawalCancellation" || t.ReferenceType == "WithdrawalRejection"));
    }

    [Fact]
    public async Task ApprovedAndPaidWithdrawals_EnforceTerminalStateMachine()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1021", 300m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 90m);

        await new ApproveWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new ApproveWithdrawalCommand { WithdrawalId = withdrawal.Id }, CancellationToken.None);

        var cancelApproved = await new CancelWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Lawyer.Id))
            .Handle(new CancelWithdrawalCommand(withdrawal.Id), CancellationToken.None);

        Assert.False(cancelApproved.IsSuccess);
        Assert.Equal("Withdrawal.InvalidStatus", cancelApproved.Error?.Code);

        var paid = await new MarkWithdrawalPaidCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new MarkWithdrawalPaidCommand
            {
                WithdrawalId = withdrawal.Id,
                PayoutReference = "PAYOUT-1021",
                PayoutProvider = "BankTransfer"
            }, CancellationToken.None);

        Assert.True(paid.IsSuccess);

        var rejectPaid = await new RejectWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new RejectWithdrawalCommand
            {
                WithdrawalId = withdrawal.Id,
                RejectionReason = "Too late"
            }, CancellationToken.None);

        var paidAgain = await new MarkWithdrawalPaidCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new MarkWithdrawalPaidCommand
            {
                WithdrawalId = withdrawal.Id,
                PayoutReference = "PAYOUT-1021-DUP"
            }, CancellationToken.None);

        Assert.False(rejectPaid.IsSuccess);
        Assert.False(paidAgain.IsSuccess);
        var finalStatus = await scope.DbContext.WithdrawalRequests
            .AsNoTracking()
            .Where(wr => wr.Id == withdrawal.Id)
            .Select(wr => wr.Status)
            .SingleAsync();
        Assert.Equal(WithdrawalStatus.Paid, finalStatus);
    }

    [Fact]
    public async Task CannotMarkPaidWithoutPayoutReference()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1022", 300m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 90m);

        await new ApproveWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new ApproveWithdrawalCommand { WithdrawalId = withdrawal.Id }, CancellationToken.None);

        var result = await new MarkWithdrawalPaidCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new MarkWithdrawalPaidCommand
            {
                WithdrawalId = withdrawal.Id,
                PayoutReference = " "
            }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Withdrawal.PayoutReferenceRequired", result.Error?.Code);
    }

    [Fact]
    public async Task WalletSummary_CountsReservedAndPaidWithdrawalsByStatus()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1023", 500m);

        var pending = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 50m);
        var approved = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 60m);
        var paid = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 70m);
        var rejected = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 80m);
        var cancelled = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 90m);

        await new ApproveWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new ApproveWithdrawalCommand { WithdrawalId = approved.Id }, CancellationToken.None);
        await new ApproveWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new ApproveWithdrawalCommand { WithdrawalId = paid.Id }, CancellationToken.None);
        await new MarkWithdrawalPaidCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new MarkWithdrawalPaidCommand
            {
                WithdrawalId = paid.Id,
                PayoutReference = "PAYOUT-1023"
            }, CancellationToken.None);
        await new RejectWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Admin.Id))
            .Handle(new RejectWithdrawalCommand
            {
                WithdrawalId = rejected.Id,
                RejectionReason = "Rejected"
            }, CancellationToken.None);
        await new CancelWithdrawalCommandHandler(scope.DbContext, new FixedCurrentUserService(seed.Lawyer.Id))
            .Handle(new CancelWithdrawalCommand(cancelled.Id), CancellationToken.None);

        var summary = await new GetMyWalletQueryHandler(scope.DbContext, new FixedCurrentUserService(seed.Lawyer.Id))
            .Handle(new GetMyWalletQuery(), CancellationToken.None);

        Assert.True(summary.IsSuccess);
        Assert.Equal(320m, summary.Value!.AvailableBalance);
        Assert.Equal(110m, summary.Value.PendingWithdrawalBalance);
        Assert.Equal(110m, summary.Value.PendingBalance);
        Assert.Equal(70m, summary.Value.PaidWithdrawalBalance);
        Assert.Equal(70m, summary.Value.TotalWithdrawn);
    }

    [Fact]
    public async Task AdminDetailReturnsFullAccountDetails_ButListsStayMasked()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var seed = await SeedLawyerWalletAsync(scope.DbContext, "1024", 300m);
        var withdrawal = await RequestWithdrawalAsync(scope.DbContext, seed.Lawyer.Id, 50m);

        var list = await new GetAdminWithdrawalsQueryHandler(scope.DbContext)
            .Handle(new GetAdminWithdrawalsQuery(), CancellationToken.None);

        var listItem = Assert.Single(list.Value!.Items);
        Assert.Equal("*******0000", listItem.AccountDetails);
        Assert.Null(listItem.AccountDetailsFull);

        var detail = await new GetAdminWithdrawalByIdQueryHandler(scope.DbContext, new FakeSensitiveDataProtector())
            .Handle(new GetAdminWithdrawalByIdQuery(withdrawal.Id), CancellationToken.None);

        Assert.True(detail.IsSuccess);
        Assert.Equal("*******0000", detail.Value!.AccountDetails);
        Assert.Equal("01000000000", detail.Value.AccountDetailsFull);
    }

    private static async Task<WithdrawalRequest> RequestWithdrawalAsync(
        AlmostasharDbContext db,
        int lawyerId,
        decimal amount)
    {
        var result = await CreateRequestWithdrawalHandler(db, lawyerId)
            .Handle(new RequestWithdrawalCommand
            {
                Amount = amount,
                Method = WithdrawalMethod.VodafoneCash,
                AccountDetails = "01000000000"
            }, CancellationToken.None);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.Error?.Message);
        }

        return await db.WithdrawalRequests.SingleAsync(wr => wr.Id == result.Value!.Id);
    }

    private static RequestWithdrawalCommandHandler CreateRequestWithdrawalHandler(
        AlmostasharDbContext db,
        int lawyerId)
    {
        return new RequestWithdrawalCommandHandler(
            db,
            new FixedCurrentUserService(lawyerId),
            new FakeSensitiveDataProtector());
    }

    private static async Task<WalletSeed> SeedLawyerWalletAsync(
        AlmostasharDbContext db,
        string suffix,
        decimal availableBalance)
    {
        var admin = TestEntityFactory.CreateAdmin($"ww-{suffix}");
        var lawyer = TestEntityFactory.CreateLawyer(suffix);
        db.AddRange(admin, lawyer);
        await db.SaveChangesAsync();

        var wallet = new Wallet
        {
            LawyerId = lawyer.Id,
            AvailableBalance = availableBalance,
            Total = availableBalance
        };
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();

        return new WalletSeed(admin, lawyer, wallet);
    }

    private sealed record WalletSeed(Admin Admin, Lawyer Lawyer, Wallet Wallet);
}
