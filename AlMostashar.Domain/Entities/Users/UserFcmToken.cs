namespace AlMostashar.Domain.Entities
{
    public class UserFcmToken : BaseEntity
    {
        // FK → User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // The FCM registration token issued by Firebase SDK on the client device
        public string Token { get; set; } = null!;

        // Optional: "android" | "ios" | "web"
        public string? DeviceType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
