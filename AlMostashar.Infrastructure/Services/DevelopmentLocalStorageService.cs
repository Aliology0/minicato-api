using System.Collections.Concurrent;
using AlMostashar.Application.Common.Enums;
using AlMostashar.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AlMostashar.Infrastructure.Services;

public sealed class DevelopmentLocalStorageService : IStorageService
{
    private sealed record DownloadGrant(string FileKey, DateTime ExpiresAtUtc);

    private readonly string _root;
    private readonly string _baseUrl;
    private readonly ConcurrentDictionary<string, DownloadGrant> _grants = new();

    public DevelopmentLocalStorageService(IConfiguration configuration)
    {
        _root = Path.Combine(Path.GetTempPath(), "AlMostashar", "documents");
        Directory.CreateDirectory(_root);
        _baseUrl = (Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
            ?? configuration["DevelopmentBaseUrl"]
            ?? "http://127.0.0.1:5099").Split(';')[0].TrimEnd('/');
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
        return $"{_baseUrl}/api/development/storage/{token}";
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
}
