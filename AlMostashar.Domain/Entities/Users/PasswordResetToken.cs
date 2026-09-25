namespace AlMostashar.Domain.Entities
{
    public class PasswordResetToken : BaseEntity
    {
        public string TokenId { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
