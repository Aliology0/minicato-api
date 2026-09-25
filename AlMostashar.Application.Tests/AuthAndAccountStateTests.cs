using AlMostashar.Application.Features.Auth.Commands.Login;
using AlMostashar.Application.Features.Auth.Commands.RefreshToken;
using AlMostashar.Application.Features.Auth.Commands.VerifyEmail;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Tests;

public class AuthAndAccountStateTests
{
    [Fact]
    public async Task Login_PendingReviewLawyer_ReturnsUserWithoutTokens()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var auth = new StubAuthService();

        var lawyer = TestEntityFactory.CreateLawyer("10", isVerified: false, status: AccountStatus.PendingReview);
        lawyer.PasswordHash = auth.HashPassword("pass123");

        db.Lawyers.Add(lawyer);
        await db.SaveChangesAsync();

        var handler = new LoginCommandHandler(db, auth);
        var result = await handler.Handle(new LoginCommand
        {
            Email = lawyer.Email,
            Password = "pass123"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.User);
        Assert.Equal(AccountStatus.PendingReview, result.Value.User.AccountStatus);
        Assert.Equal(UserRole.Lawyer, result.Value.User.Role);
        Assert.Null(result.Value.Tokens);
        Assert.False(await db.RefreshTokens.AnyAsync());
    }

    [Fact]
    public async Task Login_SuspendedLawyer_ReturnsUserWithoutTokens()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var auth = new StubAuthService();

        var lawyer = TestEntityFactory.CreateLawyer("14", isVerified: false, status: AccountStatus.Suspended);
        lawyer.PasswordHash = auth.HashPassword("pass123");

        db.Lawyers.Add(lawyer);
        await db.SaveChangesAsync();

        var handler = new LoginCommandHandler(db, auth);
        var result = await handler.Handle(new LoginCommand
        {
            Email = lawyer.Email,
            Password = "pass123"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.User);
        Assert.Equal(AccountStatus.Suspended, result.Value.User.AccountStatus);
        Assert.Equal(UserRole.Lawyer, result.Value.User.Role);
        Assert.Null(result.Value.Tokens);
        Assert.False(await db.RefreshTokens.AnyAsync());
    }

    [Fact]
    public async Task Login_ActiveLawyer_ReturnsUserWithTokens()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var auth = new StubAuthService();

        var lawyer = TestEntityFactory.CreateLawyer("15", isVerified: true, status: AccountStatus.Active);
        lawyer.PasswordHash = auth.HashPassword("pass123");

        db.Lawyers.Add(lawyer);
        await db.SaveChangesAsync();

        var handler = new LoginCommandHandler(db, auth);
        var result = await handler.Handle(new LoginCommand
        {
            Email = lawyer.Email,
            Password = "pass123"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.Tokens);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.Tokens!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Value.Tokens.RefreshToken));
        Assert.Equal(1, await db.RefreshTokens.CountAsync());
    }

    [Fact]
    public async Task RefreshToken_UnapprovedLawyer_IsBlocked_AndTokenRevoked()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var auth = new StubAuthService();

        var lawyer = TestEntityFactory.CreateLawyer("11", isVerified: false, status: AccountStatus.PendingReview);
        db.Lawyers.Add(lawyer);
        await db.SaveChangesAsync();

        var refreshToken = new RefreshToken
        {
            Token = "refresh-token-1",
            UserId = lawyer.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        var handler = new RefreshTokenCommandHandler(db, auth);
        var result = await handler.Handle(new RefreshTokenCommand(refreshToken.Token), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Auth.AccountInactive", result.Error?.Code);

        var refreshedToken = await db.RefreshTokens.FirstAsync(t => t.Id == refreshToken.Id);
        Assert.True(refreshedToken.IsRevoked);
    }

    [Fact]
    public async Task VerifyEmail_IsClientOnly_WhenLawyerIdProvided_Fails()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var auth = new StubAuthService();

        var lawyer = TestEntityFactory.CreateLawyer("12");
        db.Lawyers.Add(lawyer);
        await db.SaveChangesAsync();

        var handler = new VerifyEmailCommandHandler(db, auth);
        var result = await handler.Handle(new VerifyEmailCommand
        {
            UserId = lawyer.Id,
            OtpCode = "123456"
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Auth.InvalidOtp", result.Error?.Code);
    }

    [Fact]
    public async Task VerifyEmail_Success_SetsEmailVerifiedAndAccountStatusActive()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var auth = new StubAuthService();

        var client = TestEntityFactory.CreateClient("13", isEmailVerified: false, status: AccountStatus.EmailVerificationRequired);
        client.OTPcode = "123456";
        client.OTPcodeExpiryTime = DateTime.UtcNow.AddMinutes(5);

        db.Clients.Add(client);
        await db.SaveChangesAsync();

        var handler = new VerifyEmailCommandHandler(db, auth);
        var result = await handler.Handle(new VerifyEmailCommand
        {
            UserId = client.Id,
            OtpCode = "123456"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var refreshedClient = await db.Clients.FirstAsync(c => c.Id == client.Id);
        Assert.True(refreshedClient.IsEmailVerified);
        Assert.Equal(AccountStatus.Active, refreshedClient.AccountStatus);
        Assert.Null(refreshedClient.OTPcode);
        Assert.Null(refreshedClient.OTPcodeExpiryTime);
    }
}
