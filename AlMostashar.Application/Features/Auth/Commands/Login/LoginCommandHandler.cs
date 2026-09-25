using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using AdminEntity = AlMostashar.Domain.Entities.Admin;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = AlMostashar.Domain.Entities.RefreshToken;

using AlMostashar.Domain.Shared;

namespace AlMostashar.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IAuthService  _authService;

        public LoginCommandHandler(IAppDbContext db, IAuthService authService)
        {
            _db          = db;
            _authService = authService;
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Find user by email (case-insensitive)
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);
            var error = new Error("Auth.InvalidCredentials", Messages.Auth.InvalidCredentials);
            if (user is null)
                return Result<AuthResponseDto>.Failure(error);

            // 2. Verify password
            bool isPasswordValid = _authService.VerifyPassword(user.PasswordHash, request.Password);

            if (!isPasswordValid)
                return Result<AuthResponseDto>.Failure(error);

            // 3. Detect role + account_status from TPH discriminator
            UserRole role = user switch
            {
                AdminEntity => UserRole.Admin,
                Lawyer      => UserRole.Lawyer,
                Client      => UserRole.Client,
                _           => UserRole.Client
            };

            AccountStatus accountStatus = user switch
            {
                Client c    => c.IsEmailVerified ? AccountStatus.Active : AccountStatus.EmailVerificationRequired,
                Lawyer l    => l.AccountStatus,
                AdminEntity => AccountStatus.Active,
                _           => AccountStatus.Active
            };

            var authUser = CreateAuthUserDto(user, role, accountStatus);

            if (ShouldReturnUserWithoutTokens(user, accountStatus))
            {
                return Result<AuthResponseDto>.Success(new AuthResponseDto
                {
                    User = authUser,
                });
            }

            // 4. Non-lawyer inactive accounts remain blocked
            if (accountStatus != AccountStatus.Active)
            {
                var statusError = accountStatus switch
                {
                    AccountStatus.EmailVerificationRequired
                        => new Error("Auth.EmailNotVerified", Messages.Auth.EmailNotVerified),
                    AccountStatus.PendingReview
                        => new Error("Auth.PendingReview", "حسابك قيد المراجعة. يرجى الانتظار حتى يتم التحقق من حسابك."),
                    AccountStatus.Suspended
                        => new Error("Auth.Suspended", "تم تعليق حسابك. يرجى التواصل مع الدعم."),
                    _ => new Error("Auth.AccountInactive", "الحساب غير مفعل."),
                };
                return Result<AuthResponseDto>.Failure(statusError);
            }

            // 4. Generate JWT access token
            string jwt = _authService.GenerateJwtToken(user.Id, user.Email, user.FullName, role.ToString());

            // 5. Generate and persist refresh token
            var refreshToken = new RefreshTokenEntity
            {
                Token     = _authService.GenerateRefreshToken(),
                UserId    = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_authService.GetRefreshTokenExpiryDays()),
                CreatedAt = DateTime.UtcNow,
            };

            await _db.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 5. Return response
            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                User = authUser,
                Tokens = new AuthTokensDto
                {
                    AccessToken  = jwt,
                    RefreshToken = refreshToken.Token,
                },
            });
        }

        private static bool ShouldReturnUserWithoutTokens(User user, AccountStatus accountStatus)
            => user is Lawyer && (accountStatus == AccountStatus.PendingReview || accountStatus == AccountStatus.Suspended);

        private static AuthUserDto CreateAuthUserDto(User user, UserRole role, AccountStatus accountStatus)
        {
            return new AuthUserDto
            {
                Id            = user.Id,
                FirstName     = user.FirstName,
                LastName      = user.LastName,
                Email         = user.Email,
                Role          = role,
                AccountStatus = accountStatus,
                ProfileImage  = string.IsNullOrWhiteSpace(user.AvatarUrl) ? string.Empty : user.AvatarUrl,
                UserDetails   = user is Lawyer lawyer ? new UserDetailsDto
                {
                    Governorate = string.IsNullOrWhiteSpace(lawyer.Governorate) ? null : lawyer.Governorate,
                    City        = string.IsNullOrWhiteSpace(lawyer.City) ? null : lawyer.City,
                    Bio         = string.IsNullOrWhiteSpace(lawyer.Bio) ? null : lawyer.Bio,
                    About       = string.IsNullOrWhiteSpace(lawyer.About) ? null : lawyer.About,
                    PhoneNumber = string.IsNullOrWhiteSpace(lawyer.PhoneNo) ? string.Empty : lawyer.PhoneNo,
                } : null,
            };
        }
    }
}
