using System.Text.Json;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Locations;
using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static AlMostashar.Application.Helpers.UploadToStorage;

namespace AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile;

public sealed class UpdateLawyerProfileCommandHandler
    : IRequestHandler<UpdateLawyerProfileCommand, Result<UpdateLawyerProfileResponseDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storageService;
    private readonly ILocationCatalog? _locationCatalog;
    private readonly ILogger<UpdateLawyerProfileCommandHandler>? _logger;

    public UpdateLawyerProfileCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        IStorageService storageService,
        ILocationCatalog? locationCatalog = null,
        ILogger<UpdateLawyerProfileCommandHandler>? logger = null)
    {
        _db = db;
        _currentUser = currentUser;
        _storageService = storageService;
        _locationCatalog = locationCatalog;
        _logger = logger;
    }

    public async Task<Result<UpdateLawyerProfileResponseDto>> Handle(
        UpdateLawyerProfileCommand request, CancellationToken cancellationToken)
    {
        var lawyer = await _db.Lawyers
            .Include(l => l.LawyerSpecializations)
            .Include(l => l.PendingProfileChange)
            .FirstOrDefaultAsync(l => l.Id == _currentUser.UserId, cancellationToken);
        if (lawyer is null)
            return Failure("Lawyer.NotFound", "Lawyer not found.");

        var status = lawyer.AccountStatus;
        var isRejected = status is AccountStatus.Rejected or AccountStatus.Suspended;
        var isVerified = status == AccountStatus.Verified || (status == AccountStatus.Active && lawyer.IsVerified);
        var isPendingChanges = status == AccountStatus.PendingChangesReview;
        if (request.ResubmitForReview && !isRejected)
            return Failure("Lawyer.ResubmitInvalid", "Only a rejected lawyer can resubmit an application.");
        if (isPendingChanges && lawyer.PendingProfileChange is null)
            return Failure("Lawyer.PendingProfileMissing", "The pending profile change could not be found.");

        if (request.GovernorateId.HasValue != request.CityId.HasValue)
            return Failure("Lawyer.LocationInvalid", "GovernorateId and CityId must be supplied together.");

        RequestLocationSnapshot? location = null;
        if (request.GovernorateId.HasValue)
        {
            if (_locationCatalog is null)
                return Failure("Lawyer.LocationUnavailable", "Location validation is unavailable.");
            var locationResult = RequestLocationResolver.ResolveRequired(
                _locationCatalog, request.GovernorateId.Value, request.CityId!.Value);
            if (!locationResult.IsSuccess)
                return Result<UpdateLawyerProfileResponseDto>.Failure(locationResult.Error!);
            location = locationResult.Value;
        }

        List<LawyerSpecialization>? selectedSpecializations = null;
        if (request.SpecializationIds is not null)
        {
            if (request.SpecializationIds.Count != request.SpecializationIds.Distinct().Count())
                return Failure("Specialization.Duplicate", "Specialization IDs must be unique.");
            selectedSpecializations = await _db.LawyerSpecializations
                .Where(s => request.SpecializationIds.Contains(s.Id) && s.IsActive)
                .ToListAsync(cancellationToken);
            if (selectedSpecializations.Count != request.SpecializationIds.Count)
                return Failure("Specialization.Invalid", "One or more specialization IDs do not exist or are inactive.");
        }

        var uploadedKeys = new List<string>();
        string? avatarUrl = null;
        string? ssnKey = null;
        string? syndicateKey = null;
        try
        {
            if (request.AvatarPhoto is not null)
            {
                var key = await UploadPublicAsync(request.AvatarPhoto, _storageService);
                uploadedKeys.Add(key);
                avatarUrl = _storageService.GetPublicUrl(key);
            }
            if (request.SSNPhoto is not null)
            {
                ssnKey = await UploadAsync(request.SSNPhoto, _storageService);
                uploadedKeys.Add(ssnKey);
            }
            if (request.SyndicateCardPhoto is not null)
            {
                syndicateKey = await UploadAsync(request.SyndicateCardPhoto, _storageService);
                uploadedKeys.Add(syndicateKey);
            }
        }
        catch (Exception ex)
        {
            await CleanupUploadsAsync(uploadedKeys);
            _logger?.LogError(ex, "Lawyer profile file upload failed for lawyer {LawyerId}", lawyer.Id);
            return Failure("Upload.Failed", "Failed to upload one or more profile files.");
        }

        var useDraft = isVerified || isPendingChanges;
        LawyerProfileChange? draft = null;
        LawyerProfileChange? originalDirectProfile = null;
        if (useDraft)
        {
            draft = lawyer.PendingProfileChange;
            if (draft is null)
            {
                draft = CreateDraft(lawyer);
                lawyer.PendingProfileChange = draft;
                _db.LawyerProfileChanges.Add(draft);
            }
        }
        else
        {
            originalDirectProfile = CreateDraft(lawyer);
        }

        ApplyProfileUpdates(lawyer, draft, request, avatarUrl, ssnKey, syndicateKey, location, selectedSpecializations);

        if (useDraft)
        {
            if (MatchesApprovedProfile(draft!, lawyer))
            {
                _db.LawyerProfileChanges.Remove(draft!);
                lawyer.PendingProfileChange = null;
                lawyer.AccountStatus = AccountStatus.Verified;
            }
            else
            {
                lawyer.AccountStatus = AccountStatus.PendingChangesReview;
            }
        }
        else
        {
            var updatedDirectProfile = CreateDraft(lawyer);
            var profileChanged = !SameProfile(originalDirectProfile!, updatedDirectProfile);
            if (request.ResubmitForReview && !profileChanged)
            {
                await CleanupUploadsAsync(uploadedKeys);
                return Failure("Lawyer.ResubmitNoChanges", "Update at least one profile field before resubmitting.");
            }

            if (request.ResubmitForReview)
            {
                lawyer.AccountStatus = AccountStatus.PendingReverification;
                lawyer.RejectionReason = null;
                lawyer.NotifiedAt = null;
            }
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await CleanupUploadsAsync(uploadedKeys);
            _logger?.LogError(ex, "Failed to persist lawyer profile changes for lawyer {LawyerId}", lawyer.Id);
            if (ex is OperationCanceledException)
                throw;
            if (ex is not DbUpdateException)
                throw;
            return Failure("Lawyer.UpdateFailed", "Failed to persist profile updates.");
        }

        var responseDraft = lawyer.AccountStatus == AccountStatus.PendingChangesReview
            ? lawyer.PendingProfileChange
            : null;
        var responseSpecIds = responseDraft is null
            ? lawyer.LawyerSpecializations?.Select(s => s.Id).ToArray() ?? Array.Empty<int>()
            : DeserializeSpecializationIds(responseDraft.SpecializationIdsJson);
        var responseSpecializations = await _db.LawyerSpecializations
            .Where(s => responseSpecIds.Contains(s.Id))
            .Select(s => new SpecializationDto(s.Id, s.Title, s.ArabicTitle))
            .ToListAsync(cancellationToken);

        return Result<UpdateLawyerProfileResponseDto>.Success(new UpdateLawyerProfileResponseDto
        {
            FirstName = responseDraft?.FirstName ?? lawyer.FirstName,
            LastName = responseDraft?.LastName ?? lawyer.LastName,
            ProfileImage = responseDraft?.AvatarUrl ?? lawyer.AvatarUrl,
            PhoneNumber = responseDraft?.PhoneNo ?? lawyer.PhoneNo,
            GovernorateId = responseDraft?.GovernorateId ?? lawyer.GovernorateId,
            Governorate = responseDraft?.Governorate ?? lawyer.Governorate,
            CityId = responseDraft?.CityId ?? lawyer.CityId,
            City = responseDraft?.City ?? lawyer.City,
            SyndicateId = responseDraft?.SyndicateId ?? lawyer.SyndicateId,
            Specializations = responseSpecializations,
            YearsOfExperience = responseDraft?.YearsOfExperience ?? lawyer.YearsOfExperience,
            Bio = responseDraft?.Bio ?? lawyer.Bio,
            About = responseDraft?.About ?? lawyer.About,
            AccountStatus = lawyer.AccountStatus
        });
    }

    private static void ApplyProfileUpdates(
        Lawyer lawyer,
        LawyerProfileChange? draft,
        UpdateLawyerProfileCommand request,
        string? avatarUrl,
        string? ssnKey,
        string? syndicateKey,
        RequestLocationSnapshot? location,
        List<LawyerSpecialization>? selectedSpecializations)
    {
        if (draft is not null)
        {
            draft.FirstName = request.FirstName ?? draft.FirstName;
            draft.LastName = request.LastName ?? draft.LastName;
            draft.AvatarUrl = avatarUrl ?? draft.AvatarUrl;
            draft.YearsOfExperience = request.YearsOfExperience ?? draft.YearsOfExperience;
            draft.SSN_Url = ssnKey ?? draft.SSN_Url;
            draft.SyndicateCardUrl = syndicateKey ?? draft.SyndicateCardUrl;
            draft.PhoneNo = request.PhoneNumber ?? draft.PhoneNo;
            draft.SyndicateId = request.SyndicateId ?? draft.SyndicateId;
            draft.Bio = request.Bio ?? draft.Bio;
            draft.About = request.About ?? draft.About;
            if (location is not null)
            {
                draft.GovernorateId = location.GovernorateId;
                draft.Governorate = location.Governorate;
                draft.CityId = location.CityId;
                draft.City = location.City;
            }
            if (selectedSpecializations is not null)
                draft.SpecializationIdsJson = JsonSerializer.Serialize(selectedSpecializations.Select(s => s.Id).OrderBy(id => id));
            return;
        }

        lawyer.FirstName = request.FirstName ?? lawyer.FirstName;
        lawyer.LastName = request.LastName ?? lawyer.LastName;
        lawyer.AvatarUrl = avatarUrl ?? lawyer.AvatarUrl;
        lawyer.YearsOfExperience = request.YearsOfExperience ?? lawyer.YearsOfExperience;
        lawyer.SSN_Url = ssnKey ?? lawyer.SSN_Url;
        lawyer.SyndicateCardUrl = syndicateKey ?? lawyer.SyndicateCardUrl;
        lawyer.PhoneNo = request.PhoneNumber ?? lawyer.PhoneNo;
        lawyer.SyndicateId = request.SyndicateId ?? lawyer.SyndicateId;
        lawyer.Bio = request.Bio ?? lawyer.Bio;
        lawyer.About = request.About ?? lawyer.About;
        lawyer.FullName = $"{lawyer.FirstName} {lawyer.LastName}";
        if (location is not null)
        {
            lawyer.GovernorateId = location.GovernorateId;
            lawyer.Governorate = location.Governorate;
            lawyer.CityId = location.CityId;
            lawyer.City = location.City;
        }
        if (selectedSpecializations is not null)
            lawyer.LawyerSpecializations = selectedSpecializations;
    }

    private static LawyerProfileChange CreateDraft(Lawyer lawyer) => new()
    {
        LawyerId = lawyer.Id,
        FirstName = lawyer.FirstName,
        LastName = lawyer.LastName,
        AvatarUrl = lawyer.AvatarUrl,
        YearsOfExperience = lawyer.YearsOfExperience,
        SSN_Url = lawyer.SSN_Url,
        SyndicateCardUrl = lawyer.SyndicateCardUrl,
        PhoneNo = lawyer.PhoneNo,
        GovernorateId = lawyer.GovernorateId,
        Governorate = lawyer.Governorate,
        CityId = lawyer.CityId,
        City = lawyer.City,
        SyndicateId = lawyer.SyndicateId,
        Bio = lawyer.Bio,
        About = lawyer.About,
        SpecializationIdsJson = JsonSerializer.Serialize(lawyer.LawyerSpecializations?.Select(s => s.Id).OrderBy(id => id) ?? Enumerable.Empty<int>())
    };

    private static bool MatchesApprovedProfile(LawyerProfileChange draft, Lawyer lawyer) =>
        SameProfile(draft, CreateDraft(lawyer));

    private static bool SameProfile(LawyerProfileChange left, LawyerProfileChange right) =>
        left.FirstName == right.FirstName &&
        left.LastName == right.LastName &&
        left.AvatarUrl == right.AvatarUrl &&
        left.YearsOfExperience == right.YearsOfExperience &&
        left.SSN_Url == right.SSN_Url &&
        left.SyndicateCardUrl == right.SyndicateCardUrl &&
        left.PhoneNo == right.PhoneNo &&
        left.GovernorateId == right.GovernorateId &&
        left.Governorate == right.Governorate &&
        left.CityId == right.CityId &&
        left.City == right.City &&
        left.SyndicateId == right.SyndicateId &&
        left.Bio == right.Bio &&
        left.About == right.About &&
        DeserializeSpecializationIds(left.SpecializationIdsJson).SequenceEqual(DeserializeSpecializationIds(right.SpecializationIdsJson));

    private static int[] DeserializeSpecializationIds(string json) =>
        JsonSerializer.Deserialize<int[]>(json) ?? Array.Empty<int>();

    private async Task CleanupUploadsAsync(IEnumerable<string> keys)
    {
        foreach (var key in keys.Distinct(StringComparer.Ordinal))
        {
            try
            {
                var result = await _storageService.DeleteFileAsync(key);
                if (!result.Success)
                    _logger?.LogWarning("Failed to clean up newly uploaded lawyer profile file: {Message}", result.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to clean up a newly uploaded lawyer profile file.");
            }
        }
    }

    private static Result<UpdateLawyerProfileResponseDto> Failure(string code, string message) =>
        Result<UpdateLawyerProfileResponseDto>.Failure(new Error(code, message));
}
