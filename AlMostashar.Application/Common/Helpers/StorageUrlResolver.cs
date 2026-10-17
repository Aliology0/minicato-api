using AlMostashar.Application.Common.Interfaces;

namespace AlMostashar.Application.Common.Helpers;

public static class StorageUrlResolver
{
    public static string? ResolvePublicUrl(IStorageService storage, string? storedValue)
    {
        if (string.IsNullOrWhiteSpace(storedValue))
            return null;

        // If already a URL, return as-is
        if (storedValue.Contains("://"))
            return storedValue;

        // Otherwise treat as storage key and build public URL
        return storage.GetPublicUrl(storedValue);
    }

    public static string? ResolvePresignedUrl(IStorageService storage, string? storedValue, int expiresMinutes = 60)
    {
        if (string.IsNullOrWhiteSpace(storedValue))
            return null;

        if (storedValue.Contains("://"))
            return storedValue;

        return storage.GetPresignedUrl(storedValue, expiresMinutes);
    }
}
