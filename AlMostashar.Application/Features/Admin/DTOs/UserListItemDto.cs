using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Admin.DTOs;

public class UserListItemDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? ProfileImage { get; set; }
    public string Email { get; set; } = null!;
    public UserRole UserRole { get; set; }
    public DateTime RegisteredAt { get; set; }
    public AccountStatus AccountStatus { get; set; }

    // Lawyer-only field
    public bool? IsVerified { get; set; }
}
