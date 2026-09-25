using AlMostashar.Application.Common.Interfaces;
using MediatR;
using AlMostashar.Application.Common.Enums;

namespace AlMostashar.Application.Tests.TestSupport;

public sealed class FixedCurrentUserService : ICurrentUserService
{
    public FixedCurrentUserService(int userId) => UserId = userId;
    public int UserId { get; }
}

public sealed class RecordingStorageService : IStorageService
{
    public string UploadedKey { get; set; } = "documents/test-storage-key";
    public string? LastPresignedKey { get; private set; }
    public int? LastExpirationMinutes { get; private set; }
    public string? LastDeletedKey { get; private set; }
    public bool DeleteSucceeds { get; set; } = true;
    public Action<string>? OnDelete { get; set; }

    public Task<string> UploadFileAsync(Stream fileStream, string fileName, FileContentType contentType)
        => Task.FromResult(UploadedKey);

    public Task<string> UploadPublicFileAsync(Stream fileStream, string fileName, FileContentType contentType)
        => Task.FromResult(UploadedKey);

    public string GetPresignedUrl(string filePath, int expirationMinutes = 60)
    {
        LastPresignedKey = filePath;
        LastExpirationMinutes = expirationMinutes;
        return $"https://signed.test/{filePath}";
    }

    public string GetPublicUrl(string fileKey)
        => $"https://public.test/{fileKey}";

    public Task<(bool Success, string Message)> DeleteFileAsync(string filePath)
    {
        LastDeletedKey = filePath;
        OnDelete?.Invoke(filePath);
        return Task.FromResult(DeleteSucceeds
            ? (true, "File deleted successfully.")
            : (false, "Storage deletion failed."));
    }
}

public sealed class NoOpPublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => Task.CompletedTask;
}

public sealed class CallbackPublisher : IPublisher
{
    private readonly Func<object, CancellationToken, Task> _onPublish;

    public CallbackPublisher(Func<object, CancellationToken, Task> onPublish)
    {
        _onPublish = onPublish;
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
        => _onPublish(notification, cancellationToken);

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => _onPublish(notification!, cancellationToken);
}

public sealed class NoOpEmailService : IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

public sealed class FakeSensitiveDataProtector : ISensitiveDataProtector
{
    private const string Prefix = "test-protected:";

    public string Protect(string plainText)
    {
        return string.IsNullOrWhiteSpace(plainText)
            ? string.Empty
            : Prefix + plainText.Trim();
    }

    public string Unprotect(string protectedText)
    {
        return protectedText.StartsWith(Prefix, StringComparison.Ordinal)
            ? protectedText[Prefix.Length..]
            : protectedText;
    }

    public string Mask(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return string.Empty;
        }

        var value = plainText.Trim();
        if (value.Length <= 4)
        {
            return new string('*', value.Length);
        }

        return $"{new string('*', Math.Min(value.Length - 4, 8))}{value[^4..]}";
    }
}

public sealed class StubAuthService : IAuthService
{
    public string HashPassword(string password) => $"HASH::{password}";

    public bool VerifyPassword(string hash, string password) => hash == HashPassword(password);

    public string GenerateJwtToken(int userId, string email, string fullName, string role)
        => $"JWT::{userId}::{email}::{role}";

    public string GenerateRefreshToken() => Guid.NewGuid().ToString("N");

    public int GetRefreshTokenExpiryDays() => 7;

    public string GenerateOtp() => "123456";

    public int GetOtpExpiryMinutes() => 10;

    public (string Token, string TokenId, DateTime ExpiresAt) GenerateResetToken(int userId, string email)
    {
        var tokenId = Guid.NewGuid().ToString("N");
        return ($"RESET::{tokenId}", tokenId, DateTime.UtcNow.AddMinutes(10));
    }

    public (int UserId, string Email, string TokenId)? ValidateResetToken(string resetToken)
        => null;
}

