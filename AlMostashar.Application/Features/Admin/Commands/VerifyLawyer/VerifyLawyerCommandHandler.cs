using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Admin.Commands.VerifyLawyer
{
    public class VerifyLawyerCommandHandler : IRequestHandler<VerifyLawyerCommand, Result<string>>
    {
        private readonly IAppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ICurrentUserService _currentUserService;

        private readonly ILogger<VerifyLawyerCommandHandler> _logger;

        public VerifyLawyerCommandHandler(IAppDbContext db, IEmailService emailService, ICurrentUserService currentUserService, ILogger<VerifyLawyerCommandHandler> logger)
        {
            _db                 = db;
            _emailService       = emailService;
            _currentUserService = currentUserService;
            _logger             = logger;
        }

        public async Task<Result<string>> Handle(VerifyLawyerCommand request, CancellationToken cancellationToken)
        {
            // 1. Find the lawyer
            var lawyer = await _db.Lawyers
                .FirstOrDefaultAsync(l => l.Id == request.LawyerId, cancellationToken);

            if (lawyer is null)
                return Result<string>.Failure(new Error("Admin.NotFound", "Lawyer not found."));

            // 2. Check if already verified
            if (lawyer.IsVerified)
                return Result<string>.Failure(new Error("Admin.AlreadyVerified", "This lawyer is already verified."));

            string subject;
            string body;
            string resultMessage;
            if (request.IsAccepted)
            {
                // 3. Verify the lawyer
                lawyer.IsVerified = true;
                lawyer.VerifiedByAdminId = _currentUserService.UserId;
                lawyer.AccountStatus = Domain.ValueObject.Enum.AccountStatus.Active;

                if (!await _db.Wallets.AnyAsync(w => w.LawyerId == lawyer.Id, cancellationToken))
                {
                    _db.Wallets.Add(new Wallet
                    {
                        LawyerId = lawyer.Id,
                        AvailableBalance = 0,
                        Total = 0
                    });
                }

                subject = "Minicato - تم قبول حسابك! 🎉";
                body = $@"
                <div style='font-family: Arial, sans-serif; direction: rtl; text-align: center; padding: 20px;'>
                    <h2 style='color: #27ae60;'>تهانينا! تم قبول حسابك</h2>
                    <p>مرحباً <strong>{lawyer.FullName}</strong>،</p>
                    <p>تم مراجعة وقبول حسابك على Minicato.</p>
                    <p>يمكنك الآن تسجيل الدخول والبدء في استقبال القضايا.</p>
                    <p style='color: #888; font-size: 12px; margin-top: 20px;'>Minicato Team</p>
                </div>";
                resultMessage = $"Lawyer '{lawyer.FullName}' has been verified successfully.";
            }
            else
            {
                lawyer.AccountStatus = Domain.ValueObject.Enum.AccountStatus.Suspended;
                // Email subject for document rejection / account suspension
                subject = "Minicato - إجراء مطلوب: تحديث مستندات التحقق";

                // Email body explaining the suspension and required action
                body = $@"
                    <div style='font-family: Arial, sans-serif; direction: rtl; text-align: center; padding: 20px;'>
                        <h2 style='color: #e74c3c;'>تحديث بخصوص حالة حسابك</h2>
                        <p>مرحباً <strong>{lawyer.FullName}</strong>،</p>
                        <p>بعد مراجعة طلب انضمامك لمنصة Minicato، تبين أن ملفات التحقق المرفقة غير مكتملة أو غير واضحة.</p>
                        <p>بناءً على ذلك، سيظل حسابك <strong>معلقاً مؤقتاً</strong>.</p>
                        <p>يرجى تسجيل الدخول إلى حسابك وإعادة رفع المستندات المطلوبة بوضوح حتى نتمكن من استكمال عملية المراجعة وتفعيل الحساب.</p>
                        <p style='color: #888; font-size: 12px; margin-top: 20px;'>Minicato Team</p>
                    </div>";
                resultMessage = $"Lawyer '{lawyer.FullName}' has been rejected successfully.";
            }
            lawyer.NotifiedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            // 4. Notify the lawyer via email

            try
            {
                await _emailService.SendEmailAsync(lawyer.Email, subject, body, cancellationToken);
            }
            catch(Exception ex)
            {
                // Email failure should not block verification
                // The lawyer is already verified in DB
                _logger.LogError(ex, "Failed to send verification email to lawyer {LawyerEmail}", lawyer.Email);
                return Result<string>.Success(resultMessage + " (Note: Notification email failed to send)");
            }

            return Result<string>.Success(resultMessage);
        }
    }
}
