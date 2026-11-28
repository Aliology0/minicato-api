using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Application.Features.Auth.Commands.RegisterLawyer;
using AlMostashar.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Xunit;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Tests.Features.Auth;

public class RegisterLawyerUploadTests
{
    private IFormFile CreateFormFile(string name)
    {
        var ms = new MemoryStream(new byte[] { 1, 2, 3 });
        return new FormFile(ms, 0, ms.Length, "file", name) { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };
    }

    [Fact]
    public async Task RegisterLawyer_Fails_When_Storage_Throws()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var auth = new StubAuthService();
        var loc = TestLocationCatalog.Instance;

        var throwing = new ThrowingStorageService();

        var handler = new RegisterLawyerCommandHandler(db, auth, loc, throwing);

        var cmd = new RegisterLawyerCommand
        {
            FirstName = "T",
            LastName = "L",
            Email = "throw@example.test",
            Password = "Password1",
            PhoneNo = "010",
            GovernorateId = 1,
            CityId = 1,
            SyndicateId = 1,
            YearsOfExperience = 1,
            AvatarPhoto = CreateFormFile("a.jpg")
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("Upload.Failed", result.Error?.Code);
    }

    [Fact]
    public async Task RegisterLawyer_Succeeds_With_RecordingStorage()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var auth = new StubAuthService();
        var loc = TestLocationCatalog.Instance;

        var storage = new RecordingStorageService();
        storage.UploadedKey = "images/testkey";

        var handler = new RegisterLawyerCommandHandler(db, auth, loc, storage);

        var cmd = new RegisterLawyerCommand
        {
            FirstName = "T",
            LastName = "L",
            Email = "succeeds@example.test",
            Password = "Password1",
            PhoneNo = "010",
            GovernorateId = 1,
            CityId = 1,
            SyndicateId = 1,
            YearsOfExperience = 1,
            SSNPhoto = CreateFormFile("ssn.jpg"),
            SyndicateCardPhoto = CreateFormFile("syndicate.jpg"),
            PracticeCertificatesPhoto = CreateFormFile("practice.jpg"),
            AvatarPhoto = CreateFormFile("a.jpg")
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.True(result.IsSuccess);

        var saved = await db.Lawyers.SingleAsync(l => l.Email == "succeeds@example.test");
        Assert.Equal(storage.UploadedKey, saved.AvatarUrl);
        Assert.Equal(storage.UploadedKey, saved.SSN_Url);
        Assert.Equal(storage.UploadedKey, saved.SyndicateCardUrl);
        Assert.Equal(storage.UploadedKey, saved.PracticeCertificatesUrl);
        Assert.Equal(3, storage.PrivateUploadCount);
        Assert.Equal(1, storage.PublicUploadCount);

        var documentQuery = new AlMostashar.Application.Features.Lawyers.Queries.GetPrivateDocument.GetLawyerPrivateDocumentQueryHandler(db);
        foreach (var documentType in Enum.GetValues<AlMostashar.Application.Features.Lawyers.Queries.GetPrivateDocument.LawyerPrivateDocumentType>())
        {
            var document = await documentQuery.Handle(
                new AlMostashar.Application.Features.Lawyers.Queries.GetPrivateDocument.GetLawyerPrivateDocumentQuery(saved.Id, documentType),
                CancellationToken.None);
            Assert.True(document.IsSuccess);
            Assert.Equal(storage.UploadedKey, document.Value);
        }
    }
}

// Test helper that throws on upload
internal class ThrowingStorageService : IStorageService
{
    public Task<string> UploadFileAsync(Stream fileStream, string fileName, AlMostashar.Application.Common.Enums.FileContentType contentType)
        => throw new System.InvalidOperationException("simulated storage failure");

    public Task<string> UploadPublicFileAsync(Stream fileStream, string fileName, AlMostashar.Application.Common.Enums.FileContentType contentType)
        => throw new System.InvalidOperationException("simulated storage failure");

    public string GetPresignedUrl(string filePath, int expirationMinutes = 60) => string.Empty;
    public string GetPublicUrl(string fileKey) => string.Empty;
    public Task<(bool Success, string Message)> DeleteFileAsync(string filePath) => Task.FromResult((false, "no"));
}
