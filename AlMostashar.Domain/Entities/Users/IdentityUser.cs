namespace AlMostashar.Domain.Entities
{
    public class IdentityUser: BaseEntity
    {
        public string Email { get; set; } = null!;
        public bool IsEmailVerified { get; set; } = false;
        public string PasswordHash { get; set; } = null!;
        public string? OTPcode { get; set; } 
        public DateTime? OTPcodeExpiryTime { get; set; } = null;
    }
}
