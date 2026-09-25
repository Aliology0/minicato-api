using AlMostashar.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace AlMostashar.Infrastructure.Services;

public sealed class DataProtectionSensitiveDataProtector : ISensitiveDataProtector
{
    private const string ProtectedPrefix = "dp:v1:";
    private readonly IDataProtector _protector;

    public DataProtectionSensitiveDataProtector(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("AlMostashar.WithdrawalAccountDetails.v1");
    }

    public string Protect(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return string.Empty;
        }

        return ProtectedPrefix + _protector.Protect(plainText.Trim());
    }

    public string Unprotect(string protectedText)
    {
        if (string.IsNullOrWhiteSpace(protectedText))
        {
            return string.Empty;
        }

        if (!protectedText.StartsWith(ProtectedPrefix, StringComparison.Ordinal))
        {
            return protectedText;
        }

        return _protector.Unprotect(protectedText[ProtectedPrefix.Length..]);
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

        var visible = value[^4..];
        var hiddenLength = Math.Min(value.Length - visible.Length, 8);
        return $"{new string('*', hiddenLength)}{visible}";
    }
}
