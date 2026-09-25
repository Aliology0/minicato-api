using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using AlMostashar.Domain.Shared;
using AdminEntity = AlMostashar.Domain.Entities.Admin;
using ClientEntity = AlMostashar.Domain.Entities.Client;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = AlMostashar.Domain.Entities.RefreshToken;

namespace AlMostashar.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IAuthService  _authService;

        public RefreshTokenCommandHandler(IAppDbContext db, IAuthService authService)
        {
            _db          = db;
            _authService = authService;
        }

        public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Find the stored token, eager-load the related user
            var stored = await _db.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken);

            if (stored is null || !stored.IsActive)
                return Result<AuthResponseDto>.Failure(new Error("Auth.InvalidRefreshToken", Messages.Auth.InvalidRefreshToken));

            // 2. Detect role + account_status
            UserRole role = stored.User switch
            {
                AdminEntity => UserRole.Admin,
                Lawyer      => UserRole.Lawyer,
                ClientEntity => UserRole.Client,
                _           => UserRole.Client
            };

            AccountStatus accountStatus = stored.User switch
            {
                ClientEntity c => c.IsEmailVerified ? AccountStatus.Active : AccountStatus.EmailVerificationRequired,
                Lawyer l       => l.IsVerified ? AccountStatus.Active : AccountStatus.PendingReview,
                AdminEntity    => AccountStatus.Active,
                _              => AccountStatus.Active
            };

            // 3. Block non-Active accounts from refreshing
            if (accountStatus != AccountStatus.Active)
            {
                stored.IsRevoked = true;
                await _db.SaveChangesAsync(cancellationToken);
                return Result<AuthResponseDto>.Failure(
                    new Error("Auth.AccountInactive", "الحساب غير مفعل. لا يمكن تجديد الجلسة."));
            }

            // 4. Revoke the used token (rotation — prevents replay attacks)
            stored.IsRevoked = true;

            // 5. Create a new refresh token
            var newRefreshToken = new RefreshTokenEntity
            {
                Token     = _authService.GenerateRefreshToken(),
                UserId    = stored.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(_authService.GetRefreshTokenExpiryDays()),
                CreatedAt = DateTime.UtcNow,
            };

            await _db.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 6. Issue a new JWT
            string jwt = _authService.GenerateJwtToken(
                stored.User.Id,
                stored.User.Email,
                stored.User.FullName,
                role.ToString());

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                User = new AuthUserDto
                {
                    Id            = stored.User.Id,
                    FirstName     = stored.User.FirstName,
                    LastName      = stored.User.LastName,
                    Email         = stored.User.Email,
                    Role          = role,
                    AccountStatus = accountStatus,
                    ProfileImage  = string.IsNullOrWhiteSpace(stored.User.AvatarUrl) ? string.Empty : stored.User.AvatarUrl,
                    UserDetails   = stored.User is Lawyer lawyerUser ? new UserDetailsDto
                    {
                        Governorate = string.IsNullOrWhiteSpace(lawyerUser.Governorate) ? null : lawyerUser.Governorate,
                        City        = string.IsNullOrWhiteSpace(lawyerUser.City) ? null : lawyerUser.City,
                        Bio         = string.IsNullOrWhiteSpace(lawyerUser.Bio) ? null : lawyerUser.Bio,
                        About       = string.IsNullOrWhiteSpace(lawyerUser.About) ? null : lawyerUser.About,
                        PhoneNumber = string.IsNullOrWhiteSpace(lawyerUser.PhoneNo) ? string.Empty : lawyerUser.PhoneNo,
                    } : null,
                },
                Tokens = new AuthTokensDto
                {
                    AccessToken  = jwt,
                    RefreshToken = newRefreshToken.Token,
                },
            });
        }
    }
}
