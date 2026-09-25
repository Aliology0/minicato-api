using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = AlMostashar.Domain.Entities.RefreshToken;

namespace AlMostashar.Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result<AuthResponseDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IAuthService  _authService;

        public VerifyEmailCommandHandler(IAppDbContext db, IAuthService authService)
        {
            _db          = db;
            _authService = authService;
        }

        public async Task<Result<AuthResponseDto>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var error = new Error("Auth.InvalidOtp", Messages.Auth.InvalidOtp);

            // 1. Find user by ID — must be a Client
            var user = await _db.Clients
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
                return Result<AuthResponseDto>.Failure(error);

            // 2. Already verified?
            if (user.IsEmailVerified)
                return Result<AuthResponseDto>.Failure(new Error("Auth.AlreadyVerified", Messages.Auth.AlreadyVerified));

            // 3. Validate OTP
            if (user.OTPcode != request.OtpCode)
                return Result<AuthResponseDto>.Failure(error);

            // 4. Check expiry
            if (user.OTPcodeExpiryTime is null || user.OTPcodeExpiryTime < DateTime.UtcNow)
                return Result<AuthResponseDto>.Failure(new Error("Auth.OtpExpired", Messages.Auth.OtpExpired));

            // 5. Mark email as verified + clear OTP
            user.IsEmailVerified  = true;
            user.AccountStatus    = AccountStatus.Active;
            user.OTPcode          = null;
            user.OTPcodeExpiryTime = null;

            await _db.SaveChangesAsync(cancellationToken);

            // 6. NOW issue tokens — role derived from entity type, not hardcoded
            string jwt = _authService.GenerateJwtToken(user.Id, user.Email, user.FullName, UserRole.Client.ToString());

            var refreshToken = new RefreshTokenEntity
            {
                Token     = _authService.GenerateRefreshToken(),
                UserId    = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_authService.GetRefreshTokenExpiryDays()),
                CreatedAt = DateTime.UtcNow,
            };

            await _db.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                User = new AuthUserDto
                {
                    Id            = user.Id,
                    FirstName     = user.FirstName,
                    LastName      = user.LastName,
                    Email         = user.Email,
                    Role          = UserRole.Client,
                    AccountStatus = AccountStatus.Active,
                    ProfileImage  = string.IsNullOrWhiteSpace(user.AvatarUrl) ? string.Empty : user.AvatarUrl,
                    UserDetails   = null,
                },
                Tokens = new AuthTokensDto
                {
                    AccessToken  = jwt,
                    RefreshToken = refreshToken.Token,
                },
            });
        }
    }
}
