using AlMostashar.Application.Common.Enums;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Infrastructure.Helpers;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace AlMostashar.Infrastructure.Services;

public sealed class LocalFileStorageService : IStorageService
{
    private sealed record DownloadGrant(string FileKey, DateTime ExpiresAtUtc);

    private readonly string _publicRoot;
    private readonly string _privateRoot;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly ILogger<LocalFileStorageService>? _logger;
    private readonly ConcurrentDictionary<string, DownloadGrant> _grants = new();
    private readonly LocalStorageSettings _settings;

    public LocalFileStorageService(IConfiguration configuration, LocalStorageSettings settings, IHttpContextAccessor? httpContextAccessor = null, ILogger<LocalFileStorageService>? logger = null)
    {
        _configuration = configuration;
        _settings = settings;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;

        _publicRoot = Path.IsPathRooted(settings.PublicPath) ? settings.PublicPath : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), settings.PublicPath));
        _privateRoot = Path.IsPathRooted(settings.PrivatePath) ? settings.PrivatePath : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), settings.PrivatePath));

        Directory.CreateDirectory(_publicRoot);
        Directory.CreateDirectory(_privateRoot);
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(c, '_');
        return fileName;
    }

    private static string GetExtensionFromContentType(string fileName, FileContentType contentType)
    {
        var ext = Path.GetExtension(fileName);
        if (!string.IsNullOrWhiteSpace(ext)) return ext.ToLowerInvariant();

        return contentType.ToMimeType() switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpeg",
            "image/jpg" => ".jpg",
            "image/webp" => ".webp",
            _ => ext
        };
    }

    private string ResolvePublicPath(string fileKey) => Path.GetFullPath(Path.Combine(_publicRoot, fileKey));
    private string ResolvePrivatePath(string fileKey) => Path.GetFullPath(Path.Combine(_privateRoot, fileKey));

    private void EnsureInsideRoot(string path, string root)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid local storage key.");
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, FileContentType contentType)
    {
        // Private files (identity documents) go to private root
        var extension = GetExtensionFromContentType(fileName, contentType);
        var serverFileName = $"{Guid.NewGuid():N}{extension}";
        var destPath = ResolvePrivatePath(serverFileName);
        EnsureInsideRoot(destPath, _privateRoot);

        try
        {
            await using var dest = File.Create(destPath);
            await fileStream.CopyToAsync(dest);
            return serverFileName; // stored key
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to write private file to {Path}", destPath);
            throw;
        }
    }

    public async Task<string> UploadPublicFileAsync(Stream fileStream, string fileName, FileContentType contentType)
    {
        var extension = GetExtensionFromContentType(fileName, contentType);
        var serverFileName = $"{Guid.NewGuid():N}{extension}";
        var destPath = ResolvePublicPath(serverFileName);
        EnsureInsideRoot(destPath, _publicRoot);

        try
        {
            await using var dest = File.Create(destPath);
            await fileStream.CopyToAsync(dest);
            return serverFileName;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to write public file to {Path}", destPath);
            throw;
        }
    }

    public string GetPresignedUrl(string filePath, int expirationMinutes = 60)
    {
        var token = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        _grants[token] = new DownloadGrant(filePath, DateTime.UtcNow.AddMinutes(expirationMinutes));

        var baseUrl = BuildBaseUrl();
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Unable to determine base URL for presigned links.");

        return $"{baseUrl}/api/storage/download/{token}";
    }

    public string GetPublicUrl(string fileKey)
    {
        // Use configured public base if provided
        var configured = _settings.PublicBaseUrl;
        if (!string.IsNullOrWhiteSpace(configured))
            return configured.TrimEnd('/') + "/uploads/avatars/" + Uri.EscapeDataString(fileKey);

        var baseUrl = BuildBaseUrl();
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Unable to determine base URL for public files.");

        return $"{baseUrl}/uploads/avatars/{Uri.EscapeDataString(fileKey)}";
    }

    public async Task<(bool Success, string Message)> DeleteFileAsync(string filePath)
    {
        // Try private then public
        try
        {
            var p = ResolvePrivatePath(filePath);
            if (File.Exists(p)) { File.Delete(p); return (true, "Deleted"); }
            var q = ResolvePublicPath(filePath);
            if (File.Exists(q)) { File.Delete(q); return (true, "Deleted"); }
            return (false, "Not found");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to delete file {File}", filePath);
            return (false, ex.Message);
        }
    }

    public bool TryResolveDownload(string token, out string path)
    {
        path = string.Empty;
        if (!_grants.TryRemove(token, out var grant) || grant.ExpiresAtUtc <= DateTime.UtcNow)
            return false;

        var candidatePrivate = ResolvePrivatePath(grant.FileKey);
        if (File.Exists(candidatePrivate))
        {
            path = candidatePrivate;
            return true;
        }

        var candidatePublic = ResolvePublicPath(grant.FileKey);
        if (File.Exists(candidatePublic))
        {
            path = candidatePublic;
            return true;
        }

        return false;
    }

    private string BuildBaseUrl()
    {
        var ctx = _httpContextAccessor?.HttpContext;
        if (ctx?.Request != null)
        {
            var req = ctx.Request;
            var host = req.Host.HasValue ? req.Host.Value.TrimEnd('/') : string.Empty;
            if (!string.IsNullOrEmpty(host))
                return $"{req.Scheme}://{host}".TrimEnd('/');
        }

        var configured = _settings.PublicBaseUrl;
        if (!string.IsNullOrWhiteSpace(configured))
            return configured.Split(';')[0].TrimEnd('/');

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
        if (!string.IsNullOrWhiteSpace(env))
            return env.Split(';')[0].TrimEnd('/');

        return string.Empty;
    }
}
