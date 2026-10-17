using AlMostashar.Api.Controllers;
using AlMostashar.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using AlMostashar.Application.Common.Enums;
using Microsoft.Extensions.Configuration;

namespace AlMostashar.Application.Tests;

public class DevelopmentStorageControllerTests
{
    [Fact]
    public void Download_ReturnsFile_WithCorrectContentType()
    {
        var config = new ConfigurationBuilder().Build();
        var svc = new DevelopmentLocalStorageService(config, null);

        // create a temp file via UploadFileAsync
        var content = System.Text.Encoding.UTF8.GetBytes("hello world");
        using var ms = new MemoryStream(content);
        var keyTask = svc.UploadFileAsync(ms, "test.txt", FileContentType.Txt);
        keyTask.Wait();
        var key = keyTask.Result;

        // generate presigned URL with a fake request host so token is stored
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost:5000");
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var svcWithCtx = new DevelopmentLocalStorageService(config, accessor);
        var url = svcWithCtx.GetPresignedUrl(key, 60);

        // extract token
        var token = url.Split('/').Last();

        var services = new ServiceCollection();
        services.AddSingleton(svcWithCtx);
        var provider = services.BuildServiceProvider();

        var env = new HostingEnvironment { EnvironmentName = Environments.Development };
        var controller = new DevelopmentStorageController();

        var result = controller.Download(token, env, provider);
        Assert.IsType<Microsoft.AspNetCore.Mvc.PhysicalFileResult>(result);
        var physical = (Microsoft.AspNetCore.Mvc.PhysicalFileResult)result;
        Assert.Equal("text/plain", physical.ContentType);
    }

    [Fact]
    public void Download_ReturnsNotFound_ForInvalidToken()
    {
        var config = new ConfigurationBuilder().Build();
        var svc = new DevelopmentLocalStorageService(config, null);

        var services = new ServiceCollection();
        services.AddSingleton(svc);
        var provider = services.BuildServiceProvider();

        var env = new HostingEnvironment { EnvironmentName = Environments.Development };
        var controller = new DevelopmentStorageController();
        var result = controller.Download("no-such-token", env, provider);
        Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
    }

    private sealed class HostingEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = string.Empty;
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider? WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider? ContentRootFileProvider { get; set; }
    }
}
