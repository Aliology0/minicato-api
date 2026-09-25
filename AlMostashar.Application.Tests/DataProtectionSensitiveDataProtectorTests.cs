using AlMostashar.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;

namespace AlMostashar.Application.Tests;

public class DataProtectionSensitiveDataProtectorTests
{
    [Fact]
    public void ProtectAndUnprotect_RoundTripsSensitiveValue_AndMasksDisplayValue()
    {
        var keyDirectory = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "almostashar-dp-tests", Guid.NewGuid().ToString("N")));

        try
        {
            var provider = DataProtectionProvider.Create(
                keyDirectory,
                options => options.SetApplicationName("AlMostashar.Api"));
            var protector = new DataProtectionSensitiveDataProtector(provider);

            var protectedValue = protector.Protect("01012345678");
            var unprotectedValue = protector.Unprotect(protectedValue);
            var maskedValue = protector.Mask("01012345678");

            Assert.NotEqual("01012345678", protectedValue);
            Assert.Equal("01012345678", unprotectedValue);
            Assert.Equal("*******5678", maskedValue);
        }
        finally
        {
            keyDirectory.Delete(recursive: true);
        }
    }
}
