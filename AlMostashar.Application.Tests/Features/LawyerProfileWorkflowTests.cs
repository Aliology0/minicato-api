using System.Security.Claims;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Admin.Commands.VerifyLawyer;
using AlMostashar.Application.Features.Admin.Queries.UnVerifiedLawyers;
using AlMostashar.Application.Features.Lawyers.Commands.ChangePassword;
using AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile;
using AlMostashar.Application.Features.Lawyers.Queries.GetPrivateDocument;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Api.Controllers;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using AlMostashar.Infrastructure.Helpers;
using AlMostashar.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Tests.Features;

public sealed class LawyerProfileWorkflowTests
{
    [Fact]
    public async Task VerifiedLawyerProfileAndSpecializationChanges_AreStagedUntilAdminApproval()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var specializations = await db.LawyerSpecializations.OrderBy(s => s.Id).Take(2).ToListAsync();
        var lawyer = TestEntityFactory.CreateLawyer("profile-review", true, AccountStatus.Verified);
        lawyer.LawyerSpecializations = new List<LawyerSpecialization> { specializations[0] };
        await db.Lawyers.AddAsync(lawyer);
        var admin = TestEntityFactory.CreateAdmin("profile-review");
        await db.Admins.AddAsync(admin);
        await db.SaveChangesAsync();

        var updateHandler = new UpdateLawyerProfileCommandHandler(
            db, new FixedCurrentUserService(lawyer.Id), new RecordingStorageService(), TestLocationCatalog.Instance);
        var update = await updateHandler.Handle(new UpdateLawyerProfileCommand
        {
            FirstName = "Proposed",
            SpecializationIds = new List<int> { specializations[1].Id }
        }, CancellationToken.None);

        Assert.True(update.IsSuccess);
        Assert.Equal(AccountStatus.PendingChangesReview, update.Value!.AccountStatus);
        Assert.Null(typeof(UpdateLawyerProfileCommand).GetProperty("AccountStatus"));
        var persisted = await db.Lawyers.Include(l => l.LawyerSpecializations).SingleAsync(l => l.Id == lawyer.Id);
        Assert.Equal("Lawyer", persisted.FirstName);
        Assert.Equal(specializations[0].Id, Assert.Single(persisted.LawyerSpecializations!).Id);
        Assert.Equal("Proposed", persisted.PendingProfileChange!.FirstName);

        var queue = await new UnVerifiedLawyersQueryHandler(db).Handle(new UnVerifiedLawyersQuery(), CancellationToken.None);
        var reviewItem = Assert.Single(queue.Value!);
        Assert.Equal(AccountStatus.PendingChangesReview, reviewItem.AccountStatus);
        Assert.Equal(specializations[0].Id, Assert.Single(reviewItem.Specializations).Id);
        Assert.Equal(specializations[1].Id, Assert.Single(reviewItem.PendingChanges!.Specializations).Id);

        var verifyHandler = new VerifyLawyerCommandHandler(
            db, new NoOpEmailService(), new FixedCurrentUserService(admin.Id), NullLogger<VerifyLawyerCommandHandler>.Instance);
        var approval = await verifyHandler.Handle(new VerifyLawyerCommand { LawyerId = lawyer.Id, IsAccepted = true }, CancellationToken.None);

        Assert.True(approval.IsSuccess);
        Assert.Equal(AccountStatus.Verified, persisted.AccountStatus);
        Assert.Equal("Proposed", persisted.FirstName);
        Assert.Null(persisted.PendingProfileChange);
        Assert.Equal(specializations[1].Id, Assert.Single(persisted.LawyerSpecializations!).Id);
    }

    [Fact]
    public async Task RejectingVerifiedLawyerProfileChange_KeepsApprovedProfileAndStatus()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var lawyer = TestEntityFactory.CreateLawyer("profile-reject", true, AccountStatus.Verified);
        await db.Lawyers.AddAsync(lawyer);
        var admin = TestEntityFactory.CreateAdmin("profile-reject");
        await db.Admins.AddAsync(admin);
        await db.SaveChangesAsync();

        var updateHandler = new UpdateLawyerProfileCommandHandler(
            db, new FixedCurrentUserService(lawyer.Id), new RecordingStorageService(), TestLocationCatalog.Instance);
        var update = await updateHandler.Handle(new UpdateLawyerProfileCommand { FirstName = "Unapproved" }, CancellationToken.None);
        Assert.True(update.IsSuccess);
        Assert.Equal(AccountStatus.PendingChangesReview, update.Value!.AccountStatus);

        var verifyHandler = new VerifyLawyerCommandHandler(
            db, new NoOpEmailService(), new FixedCurrentUserService(admin.Id), NullLogger<VerifyLawyerCommandHandler>.Instance);
        var rejection = await verifyHandler.Handle(new VerifyLawyerCommand
        {
            LawyerId = lawyer.Id,
            IsAccepted = false,
            RejectionReason = "Please correct the submission"
        }, CancellationToken.None);

        Assert.True(rejection.IsSuccess);
        Assert.Equal(AccountStatus.Verified, lawyer.AccountStatus);
        Assert.Equal("Lawyer", lawyer.FirstName);
        Assert.Null(lawyer.PendingProfileChange);
        Assert.True(lawyer.IsVerified);
    }

    [Fact]
    public async Task RejectingPendingApplication_TransitionsToRejected()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var lawyer = TestEntityFactory.CreateLawyer("application-reject", false, AccountStatus.PendingReverification);
        var admin = TestEntityFactory.CreateAdmin("application-reject");
        await db.Lawyers.AddAsync(lawyer);
        await db.Admins.AddAsync(admin);
        await db.SaveChangesAsync();

        var handler = new VerifyLawyerCommandHandler(
            db, new NoOpEmailService(), new FixedCurrentUserService(admin.Id), NullLogger<VerifyLawyerCommandHandler>.Instance);
        var result = await handler.Handle(new VerifyLawyerCommand
        {
            LawyerId = lawyer.Id,
            IsAccepted = false,
            RejectionReason = "Missing document"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AccountStatus.Rejected, lawyer.AccountStatus);
        Assert.False(lawyer.IsVerified);
        Assert.Equal("Missing document", lawyer.RejectionReason);
    }

    [Fact]
    public async Task ProfileUpdate_RejectsInactiveAndDuplicateSpecializations()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var specializations = await db.LawyerSpecializations.OrderBy(s => s.Id).Take(2).ToListAsync();
        var lawyer = TestEntityFactory.CreateLawyer("profile-invalid", true, AccountStatus.Verified);
        await db.Lawyers.AddAsync(lawyer);
        await db.SaveChangesAsync();
        var handler = new UpdateLawyerProfileCommandHandler(
            db, new FixedCurrentUserService(lawyer.Id), new RecordingStorageService(), TestLocationCatalog.Instance);

        var duplicate = await handler.Handle(new UpdateLawyerProfileCommand
        {
            SpecializationIds = new List<int> { specializations[0].Id, specializations[0].Id }
        }, CancellationToken.None);
        Assert.False(duplicate.IsSuccess);
        Assert.Equal("Specialization.Duplicate", duplicate.Error!.Code);

        specializations[0].IsActive = false;
        await db.SaveChangesAsync();
        var inactive = await handler.Handle(new UpdateLawyerProfileCommand
        {
            SpecializationIds = new List<int> { specializations[0].Id }
        }, CancellationToken.None);
        Assert.False(inactive.IsSuccess);
        Assert.Equal("Specialization.Invalid", inactive.Error!.Code);
        Assert.Equal(AccountStatus.Verified, lawyer.AccountStatus);
    }

    [Fact]
    public async Task ChangePassword_WithIncorrectCurrentPassword_DoesNotChangeHashOrStatus()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var auth = new StubAuthService();
        var lawyer = TestEntityFactory.CreateLawyer("password-wrong", true, AccountStatus.Verified);
        lawyer.PasswordHash = auth.HashPassword("CurrentPass1");
        await scope.DbContext.Lawyers.AddAsync(lawyer);
        await scope.DbContext.SaveChangesAsync();
        var oldHash = lawyer.PasswordHash;
        var handler = new ChangeLawyerPasswordCommandHandler(scope.DbContext, new FixedCurrentUserService(lawyer.Id), auth);

        var result = await handler.Handle(new ChangeLawyerPasswordCommand
        {
            CurrentPassword = "IncorrectPass1",
            NewPassword = "NewPassword2",
            ConfirmNewPassword = "NewPassword2"
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Lawyer.CurrentPasswordInvalid", result.Error!.Code);
        Assert.Equal(oldHash, lawyer.PasswordHash);
        Assert.Equal(AccountStatus.Verified, lawyer.AccountStatus);
    }

    [Fact]
    public async Task ChangePassword_WithMismatchedConfirmation_DoesNotChangePassword()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var auth = new StubAuthService();
        var lawyer = TestEntityFactory.CreateLawyer("password-mismatch", true, AccountStatus.Verified);
        lawyer.PasswordHash = auth.HashPassword("CurrentPass1");
        await scope.DbContext.Lawyers.AddAsync(lawyer);
        await scope.DbContext.SaveChangesAsync();
        var oldHash = lawyer.PasswordHash;
        var handler = new ChangeLawyerPasswordCommandHandler(scope.DbContext, new FixedCurrentUserService(lawyer.Id), auth);

        var result = await handler.Handle(new ChangeLawyerPasswordCommand
        {
            CurrentPassword = "CurrentPass1",
            NewPassword = "NewPassword2",
            ConfirmNewPassword = "DifferentPass3"
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Lawyer.PasswordConfirmationMismatch", result.Error!.Code);
        Assert.Equal(oldHash, lawyer.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_Success_DoesNotTriggerProfileReview()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var auth = new StubAuthService();
        var lawyer = TestEntityFactory.CreateLawyer("password-success", true, AccountStatus.Verified);
        lawyer.PasswordHash = auth.HashPassword("CurrentPass1");
        await scope.DbContext.Lawyers.AddAsync(lawyer);
        await scope.DbContext.SaveChangesAsync();
        var handler = new ChangeLawyerPasswordCommandHandler(scope.DbContext, new FixedCurrentUserService(lawyer.Id), auth);

        var result = await handler.Handle(new ChangeLawyerPasswordCommand
        {
            CurrentPassword = "CurrentPass1",
            NewPassword = "NewPassword2",
            ConfirmNewPassword = "NewPassword2"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(auth.VerifyPassword(lawyer.PasswordHash, "NewPassword2"));
        Assert.Equal(AccountStatus.Verified, lawyer.AccountStatus);
        Assert.Null(lawyer.PendingProfileChange);
    }

    [Fact]
    public async Task UnverifiedLawyerIsBlockedFromBusinessEndpointsButCanEditProfile()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var lawyer = TestEntityFactory.CreateLawyer("middleware", false, AccountStatus.PendingReview);
        await scope.DbContext.Lawyers.AddAsync(lawyer);
        await scope.DbContext.SaveChangesAsync();
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, lawyer.Id.ToString()),
            new Claim(ClaimTypes.Role, "Lawyer")
        }, "test"));
        var nextCalled = false;
        var middleware = new AlMostashar.Api.Middlewares.LawyerVerificationMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var businessRequest = new DefaultHttpContext();
        businessRequest.User = claims;
        businessRequest.Request.Method = HttpMethods.Get;
        businessRequest.Request.Path = "/api/chats";

        await middleware.InvokeAsync(businessRequest, scope.DbContext);
        Assert.Equal(StatusCodes.Status403Forbidden, businessRequest.Response.StatusCode);
        Assert.False(nextCalled);

        var profileRequest = new DefaultHttpContext();
        profileRequest.User = claims;
        profileRequest.Request.Method = HttpMethods.Put;
        profileRequest.Request.Path = "/api/lawyers/edit-profile";
        await middleware.InvokeAsync(profileRequest, scope.DbContext);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ProfileValidator_RejectsUnsupportedAndOversizedImages()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var validator = new UpdateLawyerProfileCommandValidator(scope.DbContext);

        var unsupported = await validator.ValidateAsync(new UpdateLawyerProfileCommand
        {
            AvatarPhoto = CreateFormFile("avatar.txt", 100)
        });
        Assert.Contains(unsupported.Errors, error => error.ErrorMessage.Contains("image type", StringComparison.OrdinalIgnoreCase));

        var oversized = await validator.ValidateAsync(new UpdateLawyerProfileCommand
        {
            SSNPhoto = CreateFormFile("ssn.jpg", 5 * 1024 * 1024 + 1)
        });
        Assert.Contains(oversized.Errors, error => error.ErrorMessage.Contains("5 MB", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task LawyerCannotDownloadAnotherLawyersPrivateDocument()
    {
        var root = Path.Combine(Path.GetTempPath(), $"private-doc-test-{Guid.NewGuid():N}");
        var storage = new LocalFileStorageService(new ConfigurationBuilder().Build(), new LocalStorageSettings
        {
            PublicPath = Path.Combine(root, "public"),
            PrivatePath = Path.Combine(root, "private")
        });
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "100"), new Claim(ClaimTypes.Role, "Lawyer") }, "test"));
        var controller = new LawyerPrivateDocumentsController(
            new Mock<MediatR.IMediator>().Object,
            new FixedCurrentUserService(100),
            storage)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        try
        {
            var result = await controller.Download(200, LawyerPrivateDocumentType.Ssn, cancellationToken: CancellationToken.None);
            Assert.IsType<ForbidResult>(result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static IFormFile CreateFormFile(string fileName, long length)
    {
        var stream = new MemoryStream();
        return new FormFile(stream, 0, length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }
}
