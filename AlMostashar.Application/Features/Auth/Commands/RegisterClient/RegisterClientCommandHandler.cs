using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AlMostashar.Application.Helpers;
using AlMostashar.Application.Common.Locations;

namespace AlMostashar.Application.Features.Auth.Commands.RegisterClient
{
    public class RegisterClientCommandHandler : IRequestHandler<RegisterClientCommand, Result<RegisterClientResponseDto>>
    {
    private readonly IAppDbContext _db;
    private readonly IAuthService  _authService;
    private readonly IEmailService _emailService;
        private readonly IStorageService _storageService;
        private readonly ILocationCatalog _locationCatalog;

        public RegisterClientCommandHandler(IAppDbContext db, IAuthService authService, IEmailService emailService, IStorageService storageService, ILocationCatalog locationCatalog)
        {
            _db           = db;
            _authService  = authService;
            _emailService = emailService;
            _storageService = storageService;
            _locationCatalog = locationCatalog;
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

            // 2. Resolve location (Governorate/City)
            var locationResult = RequestLocationResolver.ResolveRequired(_locationCatalog, request.GovernorateId, request.CityId);
            if (!locationResult.IsSuccess)
                return Result<RegisterClientResponseDto>.Failure(locationResult.Error!);
            var location = locationResult.Value!;

            // 3. Build the Client entity — email NOT verified yet
            var client = new Client
            {
                FirstName        = request.FirstName,
                LastName         = request.LastName,
                FullName         = $"{request.FirstName} {request.LastName}",
                Email            = request.Email,
                PasswordHash     = _authService.HashPassword(request.Password),
                CreatedAt        = DateTime.UtcNow,
                // OTP/EMAIL VERIFICATION TEMPORARILY DISABLED
                // TODO: Re-enable email verification flags when ready for production.
                IsEmailVerified  = true, // treated as verified during temporary disable
                AccountStatus    = AlMostashar.Domain.ValueObject.Enum.AccountStatus.Active, // activate immediately
                GovernorateId    = location.GovernorateId,
                Governorate      = location.Governorate,
                CityId           = location.CityId,
                City             = location.City,
                //OTPcode          = otp,
                //OTPcodeExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes),
            };

            // 4. Persist client — NO tokens generated yet
            await _db.Clients.AddAsync(client, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 5. If files provided, upload them and update user record.
            // Avatar: public URL; ID documents: private storage keys
            try
            {
                var avatarTask = request.AvatarPhoto != null
                    ? UploadToStorage.UploadPublicAsync(request.AvatarPhoto, _storageService)
                    : Task.FromResult<string?>(null);

                var frontIdTask = request.FrontIdPhoto != null
                    ? UploadToStorage.UploadAsync(request.FrontIdPhoto, _storageService)
                    : Task.FromResult<string?>(null);

                var backIdTask = request.BackIdPhoto != null
                    ? UploadToStorage.UploadAsync(request.BackIdPhoto, _storageService)
                    : Task.FromResult<string?>(null);

                await Task.WhenAll(avatarTask, frontIdTask, backIdTask);

                var avatarKey = await avatarTask;
                var frontKey = await frontIdTask;
                var backKey = await backIdTask;

                bool updated = false;
                if (!string.IsNullOrEmpty(avatarKey))
                {
                    client.AvatarUrl = _storageService.GetPublicUrl(avatarKey!);
                    updated = true;
                }
                if (!string.IsNullOrEmpty(frontKey))
                {
                    client.FrontIdUrl = frontKey;
                    updated = true;
                }
                if (!string.IsNullOrEmpty(backKey))
                {
                    client.BackIdUrl = backKey;
                    updated = true;
                }
                // Clients do not upload syndicate membership cards — nothing to set here.

                if (updated)
                {
                    _db.Users.Update(client);
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Do not fail registration due to storage issues. Log and continue.
                // Storage exceptions should be monitored by logs.
            }

            /*
             // 5. Send verification email
             string subject = "المستشار - كود تأكيد البريد الإلكتروني";
             string body = $@"
                 <div style='font-family: Arial, sans-serif; direction: rtl; text-align: center; padding: 20px;'>
                     <h2 style='color: #2c3e50;'>مرحباً {client.FullName}!</h2>
                     <p>شكراً لتسجيلك في منصة المستشار. لتأكيد بريدك الإلكتروني، أدخل الكود التالي:</p>
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
                 Message = emailSent 
                     ? Messages.AuthSuccess.VerificationSent 
                     : "تم إنشاء الحساب بنجاح، ولكن تعذر إرسال رمز التحقق (OTP) إلى بريدك الإلكتروني. يرجى استخدام ميزة 'إعادة إرسال الرمز' للحصول على كود جديد."
             });
            */

            // OTP/EMAIL VERIFICATION TEMPORARILY DISABLED
            // TODO: Re-enable verification flow later. For now, do not send OTP and treat user as verified and active.
            return Result<RegisterClientResponseDto>.Success(new RegisterClientResponseDto
            {
                UserId = client.Id,
                Role = AlMostashar.Domain.ValueObject.Enum.UserRole.Client,
                AccountStatus = AlMostashar.Domain.ValueObject.Enum.AccountStatus.Active,
                Message = "Account created and email marked as verified (email verification temporarily disabled)."
            });
        }
    }
}

