using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.Commands.RegisterClient;
using AlMostashar.Application.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Xunit;

namespace AlMostashar.Application.Tests.Features.Auth;

public class RegistrationAndUploadsTests
{
    [Fact]
    public async Task RegisterClient_WithValidUploads_SavesUrls()
    {
        await using var scope = await TestDbContextScope.CreateAsync();
        var db = scope.DbContext;
        var storage = new RecordingStorageService();

        // Use direct handler dependencies; tests use stubbed services from TestDoubles when needed
        var auth = new StubAuthService();
        var email = new NoOpEmailService();
        var handler = new RegisterClientCommandHandler(db, auth, email, storage, TestLocationCatalog.Instance);

        var cmd = new RegisterClientCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password1",
            GovernorateId = 1,
            CityId = 1,
            AvatarPhoto = CreateFormFile("avatar.jpg"),
            FrontIdPhoto = CreateFormFile("front.jpg"),
            BackIdPhoto = CreateFormFile("back.jpg")
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.True(result.IsSuccess);
        var user = await db.Users.SingleAsync(u => u.Email == "test@example.com");
        Assert.NotNull(user.AvatarUrl);
        Assert.NotNull(user.FrontIdUrl);
        Assert.NotNull(user.BackIdUrl);
    }

    private IFormFile CreateFormFile(string name)
    {
        var ms = new MemoryStream(new byte[] { 1, 2, 3 });
        return new FormFile(ms, 0, ms.Length, "file", name) { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };
    }
}
