using System.Collections.Concurrent;
using AlMostashar.Application.Common.Enums;
using AlMostashar.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace AlMostashar.Infrastructure.Services;

public sealed class DevelopmentLocalStorageService : IStorageService
{
    private sealed record DownloadGrant(string FileKey, DateTime ExpiresAtUtc);

    private readonly string _root;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly ConcurrentDictionary<string, DownloadGrant> _grants = new();

    public DevelopmentLocalStorageService(IConfiguration configuration, IHttpContextAccessor? httpContextAccessor = null)
    {
        _root = Path.Combine(Path.GetTempPath(), "AlMostashar", "documents");
        Directory.CreateDirectory(_root);
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, FileContentType contentType)
    {
        var extension = Path.GetExtension(fileName);
        var fileKey = $"{Guid.NewGuid():N}{extension}";
        await using var destination = File.Create(Path.Combine(_root, fileKey));
        await fileStream.CopyToAsync(destination);
        return fileKey;
    }

    public Task<string> UploadPublicFileAsync(Stream fileStream, string fileName, FileContentType contentType)
    {
        return UploadFileAsync(fileStream, fileName, contentType);
    }

    public string GetPresignedUrl(string filePath, int expirationMinutes = 60)
    {
        var token = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        _grants[token] = new DownloadGrant(filePath, DateTime.UtcNow.AddMinutes(expirationMinutes));
        var baseUrl = BuildBaseUrl();
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Unable to determine development base URL. Configure DevelopmentBaseUrl or call from an HTTP request.");

        return $"{baseUrl}/api/development/storage/{token}";
    }

    public string GetPublicUrl(string fileKey)
    {
        // In development, there is no true public URL — use a presigned URL instead.
        return GetPresignedUrl(fileKey, expirationMinutes: 525600); // ~1 year
    }

    public Task<(bool Success, string Message)> DeleteFileAsync(string filePath)
    {
        var path = ResolvePath(filePath);
        if (File.Exists(path))
            File.Delete(path);
        return Task.FromResult((true, "File deleted successfully."));
    }

    public bool TryResolveDownload(string token, out string path)
    {
        path = string.Empty;
        if (!_grants.TryRemove(token, out var grant) || grant.ExpiresAtUtc <= DateTime.UtcNow)
            return false;
        var candidate = ResolvePath(grant.FileKey);
        if (!File.Exists(candidate))
            return false;
        path = candidate;
        return true;
    }

    private string ResolvePath(string fileKey)
    {
        var path = Path.GetFullPath(Path.Combine(_root, fileKey));
        var rootPrefix = Path.GetFullPath(_root).TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!path.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid local storage key.");
        return path;
    }

    private string BuildBaseUrl()
    {
        // Prefer the current request's scheme/host when available (handles dynamic ports and HTTPS)
        var ctx = _httpContextAccessor?.HttpContext;
        if (ctx?.Request != null)
        {
            var req = ctx.Request;
            var host = req.Host.HasValue ? req.Host.Value.TrimEnd('/') : string.Empty;
            if (!string.IsNullOrEmpty(host))
                return $"{req.Scheme}://{host}".TrimEnd('/');
        }

        // Next prefer an explicit configuration value
        var configured = _configuration["DevelopmentBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured.Split(';')[0].TrimEnd('/');

        // Fall back to ASPNETCORE_URLS if set
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
        if (!string.IsNullOrWhiteSpace(env))
            return env.Split(';')[0].TrimEnd('/');

        // If we reach here, we cannot determine a safe base URL.
        // Return empty to signal caller to fail explicitly and avoid misleading URLs.
        return string.Empty;
    }
}
