using System.ComponentModel.DataAnnotations.Schema;

namespace AlMostashar.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        // The opaque token value (Base64, 64 random bytes)
        public string Token { get; set; } = null!;

        // FK → User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime ExpiresAt  { get; set; }
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public bool     IsRevoked  { get; set; } = false;

        // Optional: track which device created this token
        public string? DeviceName { get; set; }

        // Helper — NotMapped to prevent EF from creating DB columns
        [NotMapped]
        public bool IsExpired  => DateTime.UtcNow >= ExpiresAt;
        [NotMapped]
        public bool IsActive   => !IsRevoked && !IsExpired;
    }
}
