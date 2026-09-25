using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Auth.Commands.ResendVerification
{
    public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, Result<string>>
    {
        private readonly IAppDbContext _db;
        private readonly IAuthService  _authService;
        private readonly IEmailService _emailService;

        public ResendVerificationCommandHandler(IAppDbContext db, IAuthService authService, IEmailService emailService)
        {
            _db           = db;
            _authService  = authService;
            _emailService = emailService;
        }

        public async Task<Result<string>> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
        {
            // 1. Find CLIENT by email — lawyers cannot use this flow
            var user = await _db.Clients
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

            // Always return success to prevent email enumeration
            if (user is null)
                return Result<string>.Success(Messages.AuthSuccess.ResendSuccess);

            // 2. Already verified — return same success message to prevent info leakage
            if (user.IsEmailVerified)
                return Result<string>.Success(Messages.AuthSuccess.ResendSuccess);

            // 3. Generate new OTP
            string otp = _authService.GenerateOtp();
            int expiryMinutes = _authService.GetOtpExpiryMinutes();

            user.OTPcode           = otp;
            user.OTPcodeExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

            await _db.SaveChangesAsync(cancellationToken);

            // 4. Send new OTP email
            string subject = "Minicato - كود تأكيد جديد";
            string body = $@"
                <div style='font-family: Arial, sans-serif; direction: rtl; text-align: center; padding: 20px;'>
                    <h2 style='color: #2c3e50;'>كود تأكيد جديد</h2>
                    <p>كود التأكيد الجديد الخاص بك هو:</p>
                    <div style='font-size: 32px; font-weight: bold; color: #27ae60; letter-spacing: 8px; padding: 20px; background: #f0f0f0; border-radius: 10px; display: inline-block;'>
                        {otp}
                    </div>
                    <p style='color: #888; margin-top: 15px;'>الكود صالح لمدة {expiryMinutes} دقائق</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, subject, body, cancellationToken);

            return Result<string>.Success(Messages.AuthSuccess.ResendSuccess);
        }
    }
}
