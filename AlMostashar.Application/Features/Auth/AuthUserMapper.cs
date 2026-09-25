using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using AdminEntity = AlMostashar.Domain.Entities.Admin;

namespace AlMostashar.Application.Features.Auth;

public static class AuthUserMapper
{
    public static UserRole GetRole(User user) => user switch
    {
        AdminEntity => UserRole.Admin,
        Lawyer => UserRole.Lawyer,
        Client => UserRole.Client,
        _ => UserRole.Client
    };

    public static AccountStatus GetAccountStatus(User user) => user switch
    {
        Client c => c.IsEmailVerified ? AccountStatus.Active : AccountStatus.EmailVerificationRequired,
        Lawyer l => l.AccountStatus,
        AdminEntity => AccountStatus.Active,
        _ => AccountStatus.Active
    };

    public static AuthUserDto ToDto(User user)
    {
        var role = GetRole(user);
        var accountStatus = GetAccountStatus(user);

        return new AuthUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = role,
            AccountStatus = accountStatus,
            VerificationStatus = user.VerificationStatus,
            ProfileImage = string.IsNullOrWhiteSpace(user.AvatarUrl) ? string.Empty : user.AvatarUrl,
            UserDetails = user is Lawyer lawyer ? new UserDetailsDto
            {
                Governorate = string.IsNullOrWhiteSpace(lawyer.Governorate) ? null : lawyer.Governorate,
                City = string.IsNullOrWhiteSpace(lawyer.City) ? null : lawyer.City,
                Bio = string.IsNullOrWhiteSpace(lawyer.Bio) ? null : lawyer.Bio,
                About = string.IsNullOrWhiteSpace(lawyer.About) ? null : lawyer.About,
                PhoneNumber = string.IsNullOrWhiteSpace(lawyer.PhoneNo) ? string.Empty : lawyer.PhoneNo,
            } : null,
        };
    }
}
