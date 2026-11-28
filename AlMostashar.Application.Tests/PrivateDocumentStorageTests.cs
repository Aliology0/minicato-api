using System.Security.Claims;
using AlMostashar.Infrastructure.Helpers;
using AlMostashar.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AlMostashar.Application.Tests;

public sealed class PrivateDocumentStorageTests
{
    [Fact]
    public void PrivateDocumentPathsRejectInvalidKeysAndDownloadTokensAreUserBound()
    {
        var root = Path.Combine(Path.GetTempPath(), "AlMostasharStorageTests", Guid.NewGuid().ToString("N"));
        var settings = new LocalStorageSettings
        {
            PublicPath = Path.Combine(root, "wwwroot", "uploads", "avatars"),
            PrivatePath = Path.Combine(root, "App_Data", "uploads", "private")
        };
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("api.example.test");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "lawyer-17") }, "test"));
        var storage = new LocalFileStorageService(
            new ConfigurationBuilder().Build(),
            settings,
            new HttpContextAccessor { HttpContext = context });

        try
        {
            var key = $"{Guid.NewGuid():N}.jpg";
            var expectedPath = Path.Combine(settings.PrivatePath, key);
            File.WriteAllText(expectedPath, "private document");

            Assert.Equal(Path.GetFullPath(expectedPath), storage.ResolvePrivateFilePath(key));
            Assert.Throws<ArgumentException>(() => storage.ResolvePrivateFilePath($"..\\{key}"));
            Assert.Throws<FileNotFoundException>(() => storage.ResolvePrivateFilePath($"{Guid.NewGuid():N}.jpg"));

            var downloadUrl = storage.GetPresignedUrl(key);
            var token = downloadUrl.Split('/').Last();
            Assert.False(storage.TryResolveDownload(token, "lawyer-18", out _));
            Assert.True(storage.TryResolveDownload(token, "lawyer-17", out var downloadedPath));
            Assert.Equal(Path.GetFullPath(expectedPath), downloadedPath);
            Assert.False(storage.TryResolveDownload(token, "lawyer-17", out _));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
