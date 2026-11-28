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
    public async Task ProfileUpdate_StoresIdentityDocumentsPrivately_AndAvatarPublicly()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var lawyer = TestEntityFactory.CreateLawyer("private-docs", isVerified: false, status: AccountStatus.PendingReview);
        await db.Lawyers.AddAsync(lawyer);
        await db.SaveChangesAsync();

        var storage = new RecordingStorageService { UploadedKey = "0123456789abcdef0123456789abcdef.jpg" };
        var handler = new AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile.UpdateLawyerProfileCommandHandler(
            db, new FixedCurrentUserService(lawyer.Id), storage);
        var command = new UpdateLawyerProfileCommand
        {
            AvatarPhoto = CreateFormFile("avatar.jpg"),
            SSNPhoto = CreateFormFile("ssn.jpg"),
            SyndicateCardPhoto = CreateFormFile("syndicate.jpg")
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, storage.PrivateUploadCount);
        Assert.Equal(1, storage.PublicUploadCount);
        var saved = await db.Lawyers.SingleAsync(l => l.Id == lawyer.Id);
        Assert.Equal(storage.UploadedKey, saved.SSN_Url);
        Assert.Equal(storage.UploadedKey, saved.SyndicateCardUrl);
        Assert.Equal($"https://public.test/{storage.UploadedKey}", saved.AvatarUrl);
    }

    private static IFormFile CreateFormFile(string fileName)
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    [Fact]
    public async Task RejectedLawyer_SeesRejectionReasonOnLogin_AndCanResubmit()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        // Arrange: create a rejected lawyer with a rejection reason
        var lawyer = TestEntityFactory.CreateLawyer("rej", isVerified: false, status: AccountStatus.Rejected);
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
        Assert.Equal(AccountStatus.Rejected, loginResult.Value.User.AccountStatus);
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
        Assert.Equal(AccountStatus.PendingReverification, refreshed!.AccountStatus);
        Assert.Null(refreshed.RejectionReason);
    }

    [Fact]
    public async Task ResubmissionWithoutFlag_DoesNotClearRejection()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;

        var lawyer = TestEntityFactory.CreateLawyer("rej2", isVerified: false, status: AccountStatus.Rejected);
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
        Assert.Equal(AccountStatus.Rejected, refreshed!.AccountStatus);
        Assert.Equal("Missing cert", refreshed.RejectionReason);
    }
}
