using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Auth.Commands.RegisterClient
{
    public class RegisterClientCommandHandler : IRequestHandler<RegisterClientCommand, Result<RegisterClientResponseDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IAuthService  _authService;
        private readonly IEmailService _emailService;

        public RegisterClientCommandHandler(IAppDbContext db, IAuthService authService, IEmailService emailService)
        {
            _db           = db;
            _authService  = authService;
            _emailService = emailService;
        }

        public async Task<Result<RegisterClientResponseDto>> Handle(RegisterClientCommand request, CancellationToken cancellationToken)
        {
            // 1. Reject if email is already registered (case-insensitive)
            bool emailExists = await _db.Users
                .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

            if (emailExists)
            {
                var error = new Error("User.Conflict", Messages.Auth.EmailAlreadyRegistered);
                return Result<RegisterClientResponseDto>.Failure(error);
            }

            // 2. Generate OTP for email verification
            string otp = _authService.GenerateOtp();
            int expiryMinutes = _authService.GetOtpExpiryMinutes();

            // 3. Build the Client entity — email NOT verified yet
            var client = new Client
            {
                FirstName        = request.FirstName,
                LastName         = request.LastName,
                FullName         = $"{request.FirstName} {request.LastName}",
                Email            = request.Email,
                PasswordHash     = _authService.HashPassword(request.Password),
                CreatedAt        = DateTime.UtcNow,
                IsEmailVerified  = false,
                AccountStatus    = AlMostashar.Domain.ValueObject.Enum.AccountStatus.EmailVerificationRequired,
                VerificationStatus = AlMostashar.Domain.ValueObject.Enum.VerificationStatus.Pending,
                AvatarUrl        = request.AvatarUrl,
                NationalIdPhotoUrl = request.NationalIdPhotoUrl,
                OTPcode          = otp,
                OTPcodeExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes),
            };

            // 4. Persist client — NO tokens generated
            await _db.Clients.AddAsync(client, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 5. Send verification email
            string subject = "Minicato - كود تأكيد البريد الإلكتروني";
            string body = $@"
                <div style='font-family: Arial, sans-serif; direction: rtl; text-align: center; padding: 20px;'>
                    <h2 style='color: #2c3e50;'>مرحباً {client.FullName}!</h2>
                    <p>شكراً لتسجيلك في Minicato. لتأكيد بريدك الإلكتروني، أدخل الكود التالي:</p>
                    <div style='font-size: 32px; font-weight: bold; color: #27ae60; letter-spacing: 8px; padding: 20px; background: #f0f0f0; border-radius: 10px; display: inline-block;'>
                        {otp}
                    </div>
                    <p style='color: #888; margin-top: 15px;'>الكود صالح لمدة {expiryMinutes} دقائق</p>
                </div>";

            bool emailSent = true;
            try
            {
                await _emailService.SendEmailAsync(client.Email, subject, body, cancellationToken);
            }
            catch (Exception)
            {
                // If email fails, the user is still saved in the DB.
                // We should not return a 500 error, as that would make the user retry and hit "Email already registered".
                // The client can request a new OTP via ResendVerification.
                emailSent = false;
            }

            // 6. Return pending response — mobile navigates to "enter OTP" screen
            return Result<RegisterClientResponseDto>.Success(new RegisterClientResponseDto
            {
                UserId = client.Id,
                Role = AlMostashar.Domain.ValueObject.Enum.UserRole.Client,
                AccountStatus = AlMostashar.Domain.ValueObject.Enum.AccountStatus.EmailVerificationRequired,
                VerificationStatus = AlMostashar.Domain.ValueObject.Enum.VerificationStatus.Pending,
                Message = emailSent 
                    ? Messages.AuthSuccess.VerificationSent 
                    : "تم إنشاء الحساب بنجاح، ولكن تعذر إرسال رمز التحقق (OTP) إلى بريدك الإلكتروني. يرجى استخدام ميزة 'إعادة إرسال الرمز' للحصول على كود جديد."
            });
        }
    }
}

