using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Admin.DTOs
{
    public class RegisterAdminResponseDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfileImage { get; set; }
        public UserRole Role { get; set; } = UserRole.Admin;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
