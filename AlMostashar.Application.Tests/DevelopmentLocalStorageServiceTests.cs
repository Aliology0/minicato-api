using System.Net;
using AlMostashar.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AlMostashar.Application.Tests;

public class DevelopmentLocalStorageServiceTests
{
    [Fact]
    public void GetPresignedUrl_UsesHttpContextHost_WhenAvailable()
    {
        var inMemorySettings = new Dictionary<string, string?>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("testhost.local:4443");

        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var svc = new DevelopmentLocalStorageService(config, accessor);

        var key = "images/test.png";
        var url = svc.GetPresignedUrl(key, expirationMinutes: 10);

        Assert.StartsWith("https://testhost.local:4443/api/development/storage/", url);
    }

    [Fact]
    public void GetPresignedUrl_UsesConfiguration_WhenNoHttpContext()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["DevelopmentBaseUrl"] = "http://dev.example:1234"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

        var svc = new DevelopmentLocalStorageService(config, httpContextAccessor: null);
        var key = "docs/test.pdf";
        var url = svc.GetPresignedUrl(key, expirationMinutes: 20);

        Assert.StartsWith("http://dev.example:1234/api/development/storage/", url);
    }

    [Fact]
    public void GetPresignedUrl_Throws_WhenNoBaseUrlAvailable()
    {
        var config = new ConfigurationBuilder().Build();
        var svc = new DevelopmentLocalStorageService(config, httpContextAccessor: null);

        var ex = Assert.Throws<InvalidOperationException>(() => svc.GetPresignedUrl("any"));
        Assert.Contains("Unable to determine development base URL", ex.Message);
    }
}
