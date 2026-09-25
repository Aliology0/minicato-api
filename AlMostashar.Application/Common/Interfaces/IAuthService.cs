namespace AlMostashar.Application.Common.Interfaces
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hash, string password);
        string GenerateJwtToken(int userId, string email, string fullName, string role);
        string GenerateRefreshToken();
        int GetRefreshTokenExpiryDays();

        // OTP & Password Reset
        string GenerateOtp();
        int GetOtpExpiryMinutes();
        (string Token, string TokenId, DateTime ExpiresAt) GenerateResetToken(int userId, string email);
        (int UserId, string Email, string TokenId)? ValidateResetToken(string resetToken);
    }
}

