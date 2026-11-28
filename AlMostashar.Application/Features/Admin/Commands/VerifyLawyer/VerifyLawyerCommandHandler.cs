using System.Text.Json;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Admin.Commands.VerifyLawyer;

public sealed class VerifyLawyerCommandHandler : IRequestHandler<VerifyLawyerCommand, Result<string>>
{
    private readonly IAppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<VerifyLawyerCommandHandler> _logger;

    public VerifyLawyerCommandHandler(IAppDbContext db, IEmailService emailService, ICurrentUserService currentUserService, ILogger<VerifyLawyerCommandHandler> logger)
    {
        _db = db;
        _emailService = emailService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(VerifyLawyerCommand request, CancellationToken cancellationToken)
    {
        var lawyer = await _db.Lawyers
            .Include(l => l.LawyerSpecializations)
            .Include(l => l.PendingProfileChange)
            .FirstOrDefaultAsync(l => l.Id == request.LawyerId, cancellationToken);
        if (lawyer is null)
            return Result<string>.Failure(new Error("Admin.NotFound", "Lawyer not found.", null, "لم يتم العثور على المحامي."));

        var status = lawyer.AccountStatus;
        var isPendingProfileChange = status == AccountStatus.PendingChangesReview;
        var isPendingApplication = status is AccountStatus.PendingReview or AccountStatus.PendingReverification;
        if (!isPendingProfileChange && !isPendingApplication)
            return Result<string>.Failure(new Error("Admin.NotPendingReview", "This lawyer has no pending review."));
        if (isPendingProfileChange && lawyer.PendingProfileChange is null)
            return Result<string>.Failure(new Error("Admin.PendingProfileMissing", "The pending profile change could not be found."));

        string subject;
        string body;
        string resultMessage;
        if (request.IsAccepted)
        {
            if (lawyer.PendingProfileChange is { } change)
            {
                lawyer.FirstName = change.FirstName;
                lawyer.LastName = change.LastName;
                lawyer.FullName = $"{change.FirstName} {change.LastName}";
                lawyer.AvatarUrl = change.AvatarUrl;
                lawyer.YearsOfExperience = change.YearsOfExperience;
                lawyer.SSN_Url = change.SSN_Url;
                lawyer.SyndicateCardUrl = change.SyndicateCardUrl;
                lawyer.PhoneNo = change.PhoneNo;
                lawyer.GovernorateId = change.GovernorateId;
                lawyer.Governorate = change.Governorate;
                lawyer.CityId = change.CityId;
                lawyer.City = change.City;
                lawyer.SyndicateId = change.SyndicateId;
                lawyer.Bio = change.Bio;
                lawyer.About = change.About;
                var specializationIds = JsonSerializer.Deserialize<int[]>(change.SpecializationIdsJson) ?? Array.Empty<int>();
                var specializations = await _db.LawyerSpecializations
                    .Where(s => specializationIds.Contains(s.Id))
                    .ToListAsync(cancellationToken);
                if (specializations.Count != specializationIds.Length)
                    return Result<string>.Failure(new Error("Admin.PendingSpecializationMissing", "A proposed specialization no longer exists."));
                lawyer.LawyerSpecializations = specializations;
                _db.LawyerProfileChanges.Remove(change);
                lawyer.PendingProfileChange = null;
            }

            lawyer.IsVerified = true;
            lawyer.VerifiedByAdminId = _currentUserService.UserId;
            lawyer.AccountStatus = AccountStatus.Verified;
            lawyer.RejectionReason = null;
            subject = "المستشار - تم قبول حسابك!";
            body = $"<p>مرحباً {lawyer.FullName}، تمت مراجعة وقبول معلومات حسابك.</p>";
            resultMessage = isPendingProfileChange
                ? $"Lawyer '{lawyer.FullName}' profile changes have been approved."
                : $"Lawyer '{lawyer.FullName}' has been verified successfully.";

            if (!await _db.Wallets.AnyAsync(w => w.LawyerId == lawyer.Id, cancellationToken))
            {
                _db.Wallets.Add(new Wallet { LawyerId = lawyer.Id, AvailableBalance = 0, Total = 0 });
            }
        }
        else if (isPendingProfileChange)
        {
            _db.LawyerProfileChanges.Remove(lawyer.PendingProfileChange!);
            lawyer.PendingProfileChange = null;
            lawyer.AccountStatus = AccountStatus.Verified;
            lawyer.IsVerified = true;
            subject = "المستشار - لم تتم الموافقة على التعديلات";
            body = $"<p>مرحباً {lawyer.FullName}، لم تتم الموافقة على التعديلات المقترحة على ملفك الشخصي. ستظل معلوماتك المعتمدة كما هي.</p>";
            resultMessage = $"Lawyer '{lawyer.FullName}' profile changes have been rejected; the approved profile was retained.";
        }
        else
        {
            lawyer.AccountStatus = AccountStatus.Rejected;
            lawyer.IsVerified = false;
            lawyer.RejectionReason = string.IsNullOrWhiteSpace(request.RejectionReason) ? null : request.RejectionReason.Trim();
            subject = "المستشار - إجراء مطلوب: تحديث بيانات التحقق";
            body = $"<p>مرحباً {lawyer.FullName}، تم رفض طلب التحقق. يرجى تحديث بياناتك وإعادة الإرسال.</p>";
            resultMessage = $"Lawyer '{lawyer.FullName}' has been rejected successfully.";
        }

        lawyer.NotifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        try
        {
            await _emailService.SendEmailAsync(lawyer.Email, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to lawyer {LawyerId}", lawyer.Id);
            return Result<string>.Success(resultMessage + " (Notification email failed to send)");
        }

        return Result<string>.Success(resultMessage);
    }
}
