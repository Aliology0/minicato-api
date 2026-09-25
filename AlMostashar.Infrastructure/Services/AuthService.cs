using AlMostashar.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AlMostashar.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);

        public bool VerifyPassword(string hash, string password)
            => BCrypt.Net.BCrypt.Verify(password, hash);

        public string GenerateJwtToken(int userId, string email, string fullName, string role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var secret = jwtSettings["Secret"]
                ?? throw new InvalidOperationException("JWT Secret is not configured.");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Name,               fullName),
                new Claim(ClaimTypes.Role,               role),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            };

            int expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out int parsed) ? parsed : 1440;

            var token = new JwtSecurityToken(
                issuer:             string.IsNullOrWhiteSpace(issuer) ? null : issuer,
                audience:           string.IsNullOrWhiteSpace(audience) ? null : audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        public int GetRefreshTokenExpiryDays()
        {
            return int.TryParse(
                _configuration["Jwt:RefreshTokenExpiryDays"], out int days) ? days : 7;
        }

        // ── OTP ─────────────────────────────────────────────────────────────

        public string GenerateOtp()
        {
            int length = int.TryParse(_configuration["Otp:Length"], out int len) ? len : 6;
            int max = (int)Math.Pow(10, length);
            int min = (int)Math.Pow(10, length - 1);
            return RandomNumberGenerator.GetInt32(min, max).ToString();
        }

        public int GetOtpExpiryMinutes()
        {
            return int.TryParse(_configuration["Otp:ExpiryMinutes"], out int minutes) ? minutes : 5;
        }

        // ── Reset Token (short-lived JWT with "password_reset" purpose) ─────

        public (string Token, string TokenId, DateTime ExpiresAt) GenerateResetToken(int userId, string email)
        {
            var secret = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret is not configured.");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenId = Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddMinutes(10);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, tokenId),
                new Claim("purpose", "password_reset"),
            };

            var token = new JwtSecurityToken(
                issuer:             string.IsNullOrWhiteSpace(issuer) ? null : issuer,
                audience:           string.IsNullOrWhiteSpace(audience) ? null : audience,
                claims:             claims,
                expires:            expiresAt, // short-lived: 10 minutes
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), tokenId, expiresAt);
        }

        public (int UserId, string Email, string TokenId)? ValidateResetToken(string resetToken)
        {
            var secret = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret is not configured.");
            var expectedIssuer = _configuration["Jwt:Issuer"];
            var expectedAudience = _configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(resetToken, new TokenValidationParameters
                {
                    // We validate signature/lifetime here and then validate issuer/audience manually from payload
                    // to avoid claim-mapping/filtering quirks observed in this flow.
                    ValidateIssuer           = false,
                    ValidateAudience         = false,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = key,
                }, out _);

                // Extract claims directly from payload JSON to avoid library claim-mapping/filtering quirks.
                var parts = resetToken.Split('.');
                if (parts.Length < 2) return null;

                var payloadJson = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(parts[1]));
                using var payloadDoc = JsonDocument.Parse(payloadJson);
                var root = payloadDoc.RootElement;

                if (!string.IsNullOrWhiteSpace(expectedIssuer))
                {
                    var tokenIssuer = root.TryGetProperty(JwtRegisteredClaimNames.Iss, out var issElement)
                        ? issElement.GetString()
                        : null;
                    if (!string.Equals(tokenIssuer, expectedIssuer, StringComparison.Ordinal))
                        return null;
                }

                if (!string.IsNullOrWhiteSpace(expectedAudience))
                {
                    if (!root.TryGetProperty(JwtRegisteredClaimNames.Aud, out var audElement))
                        return null;

                    var audienceValid = audElement.ValueKind switch
                    {
                        JsonValueKind.String => string.Equals(audElement.GetString(), expectedAudience, StringComparison.Ordinal),
                        JsonValueKind.Array => audElement.EnumerateArray()
                            .Any(a => a.ValueKind == JsonValueKind.String &&
                                      string.Equals(a.GetString(), expectedAudience, StringComparison.Ordinal)),
                        _ => false,
                    };

                    if (!audienceValid)
                        return null;
                }

                var purpose = root.TryGetProperty("purpose", out var purposeElement) ? purposeElement.GetString() : null;
                if (purpose != "password_reset") return null;

                var userIdStr = root.TryGetProperty(JwtRegisteredClaimNames.Sub, out var subElement) ? subElement.GetString() : null;
                var email     = root.TryGetProperty(JwtRegisteredClaimNames.Email, out var emailElement) ? emailElement.GetString() : null;
                var tokenId   = root.TryGetProperty(JwtRegisteredClaimNames.Jti, out var jtiElement) ? jtiElement.GetString() : null;

                if (userIdStr is null || email is null || tokenId is null || !int.TryParse(userIdStr, out int userId))
                    return null;

                return (userId, email, tokenId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Reset token validation failed.");
                return null;
            }
        }
    }
}

