using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Auth.DTOs
{
    /// <summary>
    /// Returned after a lawyer registers successfully.
    /// The lawyer is always pending admin verification at this stage,
    /// so NO tokens are issued — they cannot use the app yet.
    /// The mobile app uses AccountStatus to navigate to a "pending review" screen.
    /// </summary>
    public class RegisterLawyerResponseDto
    {
        public int UserId { get; set; }
        public UserRole Role { get; set; } = UserRole.Lawyer;
        public AccountStatus AccountStatus { get; set; } = AccountStatus.PendingReview;
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public int ExpectedReviewDays { get; set; } = 2;
    }
}


