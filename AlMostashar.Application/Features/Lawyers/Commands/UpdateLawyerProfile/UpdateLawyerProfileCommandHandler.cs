using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static AlMostashar.Application.Helpers.UploadToStorage;

namespace AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile;

public class UpdateLawyerProfileCommandHandler
    : IRequestHandler<UpdateLawyerProfileCommand, Result<UpdateLawyerProfileResponseDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storageService;

    public UpdateLawyerProfileCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        IStorageService storageService)
    {
        _db = db;
        _currentUser = currentUser;
        _storageService = storageService;
    }

    public async Task<Result<UpdateLawyerProfileResponseDto>> Handle(
        UpdateLawyerProfileCommand request, CancellationToken cancellationToken)
    {

        // 1. Load the current lawyer
        var lawyer = await _db.Lawyers
            .Include(l => l.LawyerSpecializations)
            .FirstOrDefaultAsync(l => l.Id == _currentUser.UserId, cancellationToken);

        if (lawyer is null)
        {
            return Result<UpdateLawyerProfileResponseDto>.Failure(
                new Error("Lawyer.NotFound", Messages.Generic.NotFound(Messages.Fields.Lawyer)));
        }

        // 2. Upload profile image and documents if provided
        // Track newly uploaded storage keys so we can clean them up if later uploads or DB save fail.
        var uploadedKeys = new List<string>();

        var oldAvatar = lawyer.AvatarUrl;
        var oldSsn = lawyer.SSN_Url;
        var oldSynd = lawyer.SyndicateCardUrl;
        var oldPractice = lawyer.PracticeCertificatesUrl;

        var documentReplaced = false;

        try
        {
            if (request.ProfileImage is not null)
            {
                var fileKey = await UploadPublicAsync(request.ProfileImage, _storageService);
                uploadedKeys.Add(fileKey);
                lawyer.AvatarUrl = _storageService.GetPublicUrl(fileKey);
            }

            if (request.SSNPhoto is not null)
            {
                var ssnKey = await UploadPublicAsync(request.SSNPhoto, _storageService);
                uploadedKeys.Add(ssnKey);
                var ssnUrl = _storageService.GetPublicUrl(ssnKey);
                lawyer.SSN_Url = ssnUrl;
                documentReplaced = true;
            }

            if (request.SyndicateCardPhoto is not null)
            {
                var syndKey = await UploadPublicAsync(request.SyndicateCardPhoto, _storageService);
                uploadedKeys.Add(syndKey);
                var syndUrl = _storageService.GetPublicUrl(syndKey);
                lawyer.SyndicateCardUrl = syndUrl;
                documentReplaced = true;
            }

            if (request.PracticeCertificatesPhoto is not null)
            {
                var pracKey = await UploadPublicAsync(request.PracticeCertificatesPhoto, _storageService);
                uploadedKeys.Add(pracKey);
                var pracUrl = _storageService.GetPublicUrl(pracKey);
                lawyer.PracticeCertificatesUrl = pracUrl;
                documentReplaced = true;
            }
        }
        catch (Exception ex)
        {
            // If any upload failed, attempt to delete any files we already uploaded during this request.
            foreach (var k in uploadedKeys)
            {
                try { await _storageService.DeleteFileAsync(k); } catch { /* best-effort cleanup */ }
            }

            // Restore in-memory previous URLs so the tracked entity isn't left with partial values.
            lawyer.AvatarUrl = oldAvatar;
            lawyer.SSN_Url = oldSsn;
            lawyer.SyndicateCardUrl = oldSynd;
            lawyer.PracticeCertificatesUrl = oldPractice;

            return Result<UpdateLawyerProfileResponseDto>.Failure(new Error("Upload.Failed", "Failed to upload one or more files."));
        }

        // 3. Update fields
            lawyer.FirstName = request.FirstName ?? lawyer.FirstName;
            lawyer.LastName= request.LastName ?? lawyer.LastName;
            lawyer.FullName= $"{request.FirstName?? lawyer.FirstName} {request.LastName?? lawyer.LastName}";
            lawyer.YearsOfExperience = request.YearsOfExperience ?? lawyer.YearsOfExperience;
            lawyer.Bio= request.Bio ?? lawyer.Bio;
            lawyer.About= request.About ?? lawyer.About;
        List<SpecializationDto> Specializations = null!;
        if (request.SpecializationIds != null)
        {
            var existingSpecializations = await _db.LawyerSpecializations
                .Where(s => request.SpecializationIds.Contains(s.Id))
                .ToListAsync(cancellationToken);
                
            lawyer.LawyerSpecializations ??= new List<Domain.Entities.LawyerSpecialization>();
            lawyer.LawyerSpecializations.Clear();
            Specializations = new();
            foreach (var spec in existingSpecializations)
            {
                lawyer.LawyerSpecializations.Add(spec);
                Specializations.Add(new SpecializationDto(spec.Id, spec.Title, spec.ArabicTitle));
            }
        }

        // 4. If any required document was replaced, reset verification state (do not touch email verification)
        if (documentReplaced && lawyer.IsVerified)
        {
            lawyer.IsVerified = false;
            lawyer.VerifiedByAdminId = null;
            lawyer.AccountStatus = AlMostashar.Domain.ValueObject.Enum.AccountStatus.PendingReview;
            lawyer.NotifiedAt = null;
        }

        // 4b. Explicit resubmission flow: if user is currently rejected (Suspended) and requests resubmission,
        // validate that this is an intentional action and that the caller provided ResubmitForReview = true.
        var isRejected = lawyer.AccountStatus == AlMostashar.Domain.ValueObject.Enum.AccountStatus.Suspended
                         && !string.IsNullOrWhiteSpace(lawyer.RejectionReason);
        if (request.ResubmitForReview)
        {
            if (!isRejected)
            {
                return Result<UpdateLawyerProfileResponseDto>.Failure(new Error("Lawyer.ResubmitInvalid", "Cannot resubmit: lawyer is not currently rejected."));
            }

            // Only transition to PendingReview after successful uploads and field updates above.
            lawyer.AccountStatus = AlMostashar.Domain.ValueObject.Enum.AccountStatus.PendingReview;
            lawyer.NotifiedAt = null;
            // Clear active rejection reason now that resubmission has been made. We will only persist this when SaveChanges succeeds.
            lawyer.RejectionReason = null;
        }

        // 5. Save (persist both uploaded URLs and status change atomically)
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // Attempt best-effort cleanup of newly uploaded files if DB save failed.
            foreach (var k in uploadedKeys)
            {
                try { await _storageService.DeleteFileAsync(k); } catch { /* best-effort cleanup */ }
            }

            // Restore persisted values in the tracked entity so in-memory state is consistent with DB on failure.
            lawyer.AvatarUrl = oldAvatar;
            lawyer.SSN_Url = oldSsn;
            lawyer.SyndicateCardUrl = oldSynd;
            lawyer.PracticeCertificatesUrl = oldPractice;

            // If we attempted to clear rejection reason for resubmission, restore it because the save failed.
            if (isRejected && request.ResubmitForReview)
            {
                // re-fetch persisted rejection reason to avoid overwriting with null
                var persisted = await _db.Lawyers.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lawyer.Id, cancellationToken);
                if (persisted is not null)
                    lawyer.RejectionReason = persisted.RejectionReason;
            }

            return Result<UpdateLawyerProfileResponseDto>.Failure(new Error("Lawyer.UpdateFailed", "Failed to persist profile updates."));
        }

        // 5. Return updated profile
        return Result<UpdateLawyerProfileResponseDto>.Success(new UpdateLawyerProfileResponseDto
        {
            ProfileImage = lawyer.AvatarUrl,
            FirstName = lawyer.FirstName,
            LastName = lawyer.LastName,
            Specializations = Specializations,
            YearsOfExperience = lawyer.YearsOfExperience,
            Bio = lawyer.Bio,
            About = lawyer.About
        });
    }
}
