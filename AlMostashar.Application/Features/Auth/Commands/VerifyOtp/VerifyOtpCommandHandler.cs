using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Auth.Commands.VerifyOtp
{
    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<VerifyOtpResponseDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IAuthService  _authService;

        public VerifyOtpCommandHandler(IAppDbContext db, IAuthService authService)
        {
            _db          = db;
            _authService = authService;
        }

        public async Task<Result<VerifyOtpResponseDto>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var error = new Error("Auth.InvalidOtp", Messages.Auth.InvalidOtp);

            // 1. Find user by email
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

            if (user is null)
                return Result<VerifyOtpResponseDto>.Failure(error);

            // 1.5. Reject unverified clients
            if (user is AlMostashar.Domain.Entities.Client && !user.IsEmailVerified)
                return Result<VerifyOtpResponseDto>.Failure(error);

            // 2. Validate OTP
            if (user.OTPcode != request.OtpCode)
                return Result<VerifyOtpResponseDto>.Failure(error);

            // 3. Check expiry
            if (user.OTPcodeExpiryTime is null || user.OTPcodeExpiryTime < DateTime.UtcNow)
                return Result<VerifyOtpResponseDto>.Failure(new Error("Auth.OtpExpired", Messages.Auth.OtpExpired));

            // 4. Clear OTP (single-use)
            user.OTPcode           = null;
            user.OTPcodeExpiryTime = null;
            await _db.SaveChangesAsync(cancellationToken);

            // 5. Generate short-lived reset token and persist it as one-time-use
            var resetToken = _authService.GenerateResetToken(user.Id, user.Email);
            await _db.PasswordResetTokens.AddAsync(new AlMostashar.Domain.Entities.PasswordResetToken
            {
                TokenId = resetToken.TokenId,
                UserId = user.Id,
                ExpiresAt = resetToken.ExpiresAt,
                CreatedAt = DateTime.UtcNow,
                IsUsed = false,
            }, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<VerifyOtpResponseDto>.Success(new VerifyOtpResponseDto
            {
                ResetToken = resetToken.Token,
            });
        }
    }
}
