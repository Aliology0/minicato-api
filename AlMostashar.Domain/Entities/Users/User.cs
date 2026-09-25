using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

        // Navigation — Step 4: 1:N (User → Notification)
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // Navigation — Step 2: Weak Entity (User participates in Chats)
        public ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();

        // Navigation — 1:N (User as Reporter → Reports Filed)
        public ICollection<Report> ReportsFiled { get; set; } = new List<Report>();

        // Navigation — 1:N (User as Reported → Reports Received)
        public ICollection<Report> ReportsReceived { get; set; } = new List<Report>();

        // Navigation — 1:N (User → RefreshTokens for multi-device auth)
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        // Navigation — 1:N (User → PasswordResetTokens for single-use reset flow)
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

        // Navigation — 1:N (User → FcmTokens for push notifications per device)
        public ICollection<UserFcmToken> FcmTokens { get; set; } = new List<UserFcmToken>();

    }
}
