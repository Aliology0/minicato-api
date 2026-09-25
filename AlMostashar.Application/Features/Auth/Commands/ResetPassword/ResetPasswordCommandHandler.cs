using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        private readonly IAppDbContext _db;
        private readonly IAuthService  _authService;

        public ResetPasswordCommandHandler(IAppDbContext db, IAuthService authService)
        {
            _db          = db;
            _authService = authService;
        }

        public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate the reset token
            var tokenData = _authService.ValidateResetToken(request.ResetToken);

            if (tokenData is null)
                return Result<string>.Failure(new Error("Auth.InvalidResetToken", Messages.Auth.InvalidResetToken));

            var (userId, email, tokenId) = tokenData.Value;

            // 2. Find user
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Email.ToLower() == email.ToLower(), cancellationToken);

            if (user is null)
                return Result<string>.Failure(new Error("Auth.NotFound", Messages.Auth.UserNotFound));

            // 3. Ensure token is issued, not expired and not used (single-use)
            var consumedRows = await _db.ConsumePasswordResetTokenAsync(tokenId, user.Id, DateTime.UtcNow, cancellationToken);
            if (consumedRows == 0)
                return Result<string>.Failure(new Error("Auth.InvalidResetToken", Messages.Auth.InvalidResetToken));

            // 4. Update password + consume reset token
            user.PasswordHash = _authService.HashPassword(request.NewPassword);

            // 5. Revoke all active refresh tokens to force re-login on all devices
            var activeRefreshTokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var activeToken in activeRefreshTokens)
            {
                activeToken.IsRevoked = true;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(Messages.AuthSuccess.PasswordReset);
        }
    }
}
