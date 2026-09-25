using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Auth.DTOs
{
    public class AuthUserDto
    {
        public int     Id             { get; set; }
        public string  FirstName      { get; set; } = null!;
        public string  LastName       { get; set; } = null!;
        public string  Email          { get; set; } = null!;
        public UserRole  Role           { get; set; }
        /// <summary>Uses <see cref="AccountStatus"/> enum values.</summary>
        public AccountStatus  AccountStatus  { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public string? ProfileImage   { get; set; }
        /// <summary>
        /// Lawyer-specific details (governorate, city, bio, about).
        /// Null for Client and Admin accounts.
        /// </summary>
        public UserDetailsDto? UserDetails { get; set; }
    }
}
