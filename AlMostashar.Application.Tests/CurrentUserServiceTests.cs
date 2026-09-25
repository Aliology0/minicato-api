using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AlMostashar.Api.Services;

using Microsoft.AspNetCore.Http;

namespace AlMostashar.Application.Tests;

public class CurrentUserServiceTests
{
    [Fact]
    public void UserId_UsesSubClaim_WhenNameIdentifierIsMissing()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(JwtRegisteredClaimNames.Sub, "42") },
                "TestAuth"))
        };

        var accessor = new HttpContextAccessor { HttpContext = context };
        var service = new CurrentUserService(accessor);

        Assert.Equal(42, service.UserId);
    }

    [Fact]
    public void UserId_Throws_WhenNoUsableClaimExists()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        var accessor = new HttpContextAccessor { HttpContext = context };
        var service = new CurrentUserService(accessor);

        Assert.Throws<UnauthorizedAccessException>(() => _ = service.UserId);
    }
}

