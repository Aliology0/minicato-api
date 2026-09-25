using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
    {
        private readonly IAppDbContext  _db;
        private readonly IAuthService   _authService;
        private readonly IEmailService  _emailService;

        public ForgotPasswordCommandHandler(IAppDbContext db, IAuthService authService, IEmailService emailService)
        {
            _db           = db;
            _authService  = authService;
            _emailService = emailService;
        }

        public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // 1. Find user by email
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

            // Always return success to prevent email enumeration
            if (user is null)
                return Result<string>.Success(Messages.AuthSuccess.OtpSent);

            // 2. Reject unverified clients — can't reset password for unconfirmed email
            if (user is AlMostashar.Domain.Entities.Client && !user.IsEmailVerified)
                return Result<string>.Success(Messages.AuthSuccess.OtpSent);

            // 2. Generate OTP and set expiry
            string otp = _authService.GenerateOtp();
            int expiryMinutes = _authService.GetOtpExpiryMinutes();

            user.OTPcode           = otp;
            user.OTPcodeExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

            await _db.SaveChangesAsync(cancellationToken);

            // 3. Send OTP email
            string subject = "Minicato - كود تأكيد إعادة تعيين كلمة المرور";
            string body = $@"
                <div style='font-family: Arial, sans-serif; direction: rtl; text-align: center; padding: 20px;'>
                    <h2 style='color: #2c3e50;'>إعادة تعيين كلمة المرور</h2>
                    <p>كود التأكيد الخاص بك هو:</p>
                    <div style='font-size: 32px; font-weight: bold; color: #27ae60; letter-spacing: 8px; padding: 20px; background: #f0f0f0; border-radius: 10px; display: inline-block;'>
                        {otp}
                    </div>
                    <p style='color: #888; margin-top: 15px;'>الكود صالح لمدة {expiryMinutes} دقائق</p>
                    <p style='color: #ccc; font-size: 12px;'>إذا لم تطلب إعادة تعيين كلمة المرور، تجاهل هذا البريد.</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, subject, body, cancellationToken);

            return Result<string>.Success(Messages.AuthSuccess.OtpSent);
        }
    }
}
