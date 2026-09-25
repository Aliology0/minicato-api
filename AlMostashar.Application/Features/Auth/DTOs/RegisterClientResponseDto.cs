using AlMostashar.Application.Common.Constants;
using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Auth.DTOs
{
    /// <summary>
    /// Returned after a client registers successfully.
    /// The client must verify their email before receiving tokens.
    /// The mobile app uses AccountStatus to navigate to an "enter OTP" screen.
    /// </summary>
    public class RegisterClientResponseDto
    {
        public int UserId { get; set; }
        public UserRole Role { get; set; } = UserRole.Client;
        public AccountStatus AccountStatus { get; set; } = AccountStatus.EmailVerificationRequired;
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public string Message { get; set; } = Messages.AuthSuccess.VerificationSent;
    }
}



