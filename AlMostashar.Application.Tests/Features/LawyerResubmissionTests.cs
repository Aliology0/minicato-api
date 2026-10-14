using AlMostashar.Application.Features.Auth.Commands.Login;
using AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlMostashar.Application.Tests.Features;

public class LawyerResubmissionTests
{
    [Fact]
    public async Task RejectedLawyer_SeesRejectionReasonOnLogin_AndCanResubmit()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        // Arrange: create a rejected lawyer with a rejection reason
        var lawyer = TestEntityFactory.CreateLawyer("rej", isVerified: false, status: AccountStatus.Suspended);
        lawyer.RejectionReason = "Docs blurry";
        await db.Lawyers.AddAsync(lawyer);
        await db.SaveChangesAsync();

        var authSvc = new StubAuthService();
        // Ensure the stored password matches the stub auth service for the test login
        lawyer.PasswordHash = authSvc.HashPassword("password");
        db.Update(lawyer);
        await db.SaveChangesAsync();
        var loginHandler = new AlMostashar.Application.Features.Auth.Commands.Login.LoginCommandHandler(db, authSvc);

        // Act: login
        var loginResult = await loginHandler.Handle(new LoginCommand { Email = lawyer.Email, Password = "password" }, CancellationToken.None);
        Assert.True(loginResult.IsSuccess);
        Assert.Equal(AccountStatus.Suspended, loginResult.Value.User.AccountStatus);
        Assert.Equal("Docs blurry", loginResult.Value.User.UserDetails!.RejectionReason);

        // Now resubmit via UpdateLawyerProfileCommandHandler
        var storage = new RecordingStorageService();
        var updateHandler = new AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile.UpdateLawyerProfileCommandHandler(db, new FixedCurrentUserService(lawyer.Id), storage);

        var cmd = new UpdateLawyerProfileCommand
        {
            FirstName = "Updated",
            ResubmitForReview = true
        };

        var res = await updateHandler.Handle(cmd, CancellationToken.None);
        Assert.True(res.IsSuccess);

        var refreshed = await db.Lawyers.FindAsync(lawyer.Id);
        Assert.Equal(AccountStatus.PendingReview, refreshed!.AccountStatus);
        Assert.Null(refreshed.RejectionReason);
    }

    [Fact]
    public async Task ResubmissionWithoutFlag_DoesNotClearRejection()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var lawyer = TestEntityFactory.CreateLawyer("rej2", isVerified: false, status: AccountStatus.Suspended);
        lawyer.RejectionReason = "Missing cert";
        await db.Lawyers.AddAsync(lawyer);
        await db.SaveChangesAsync();

        var storage = new RecordingStorageService();
        var updateHandler = new AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile.UpdateLawyerProfileCommandHandler(db, new FixedCurrentUserService(lawyer.Id), storage);

        var cmd = new UpdateLawyerProfileCommand
        {
            FirstName = "UpdatedNoResubmit",
            ResubmitForReview = false
        };

        var res = await updateHandler.Handle(cmd, CancellationToken.None);
        Assert.True(res.IsSuccess);

        var refreshed = await db.Lawyers.FindAsync(lawyer.Id);
        Assert.Equal(AccountStatus.Suspended, refreshed!.AccountStatus);
        Assert.Equal("Missing cert", refreshed.RejectionReason);
    }
}
