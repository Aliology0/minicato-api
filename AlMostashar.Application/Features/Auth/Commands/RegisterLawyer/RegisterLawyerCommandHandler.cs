using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Entities;

using AlMostashar.Domain.Shared;
using AlMostashar.Application.Common.Locations;
using AlMostashar.Application.Helpers;

namespace AlMostashar.Application.Features.Auth.Commands.RegisterLawyer
{
    public class RegisterLawyerCommandHandler : IRequestHandler<RegisterLawyerCommand, Result<RegisterLawyerResponseDto>>
    {
    private readonly IAppDbContext _db;
    private readonly IAuthService  _authService;
    private readonly ILocationCatalog _locationCatalog;
    private readonly IStorageService? _storageService;
    private readonly ILogger<RegisterLawyerCommandHandler>? _logger;
        public RegisterLawyerCommandHandler(IAppDbContext db, IAuthService authService, ILocationCatalog locationCatalog, IStorageService? storageService = null, ILogger<RegisterLawyerCommandHandler>? logger = null)
    {
        _db = db;
        _authService = authService;
        _locationCatalog = locationCatalog;
        _storageService = storageService;
        _logger = logger;
    }

        public async Task<Result<RegisterLawyerResponseDto>> Handle(RegisterLawyerCommand request, CancellationToken cancellationToken)
        {

            // 1. Reject if email is already registered (case-insensitive)
            bool emailExists = await _db.Users
                .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

            var locationResult = RequestLocationResolver.ResolveRequired(
                _locationCatalog,
                request.GovernorateId,
                request.CityId);

            if (!locationResult.IsSuccess)
                return Result<RegisterLawyerResponseDto>.Failure(locationResult.Error!);

            var location = locationResult.Value!;

            if (emailExists)
            {
                var error = new Error("User.Conflict", Messages.Auth.EmailAlreadyRegistered);
                return Result<RegisterLawyerResponseDto>.Failure(error);
            }

            // 1.5. Validate and load specializations if provided
            List<Domain.Entities.LawyerSpecialization>? selectedSpecializations = null;
            if (request.SpecializationIds != null && request.SpecializationIds.Any())
            {
                selectedSpecializations = await _db.LawyerSpecializations
                    .Where(s => request.SpecializationIds.Contains(s.Id))
                    .ToListAsync(cancellationToken);

                // Verify all requested IDs exist
                if (selectedSpecializations.Count != request.SpecializationIds.Count)
                {
                    return Result<RegisterLawyerResponseDto>.Failure(
                        new Error("Specialization.Invalid", 
                            "One or more specialization IDs do not exist."));
                }
            }

            // 2. If files provided, upload them and set URLs. Registration must fail on any supplied-file upload failure.
            string? ssnKey = null;
            string? syndicateKey = null;
            string? avatarKey = null;
            var uploadedKeys = new List<string>();
            try
            {
                if (request.SSNPhoto is not null)
                {
                    ssnKey = await UploadOptionalAsync(request.SSNPhoto, false);
                    if (ssnKey is null)
                        throw new InvalidOperationException("SSN upload did not return a storage key.");
                    uploadedKeys.Add(ssnKey);
                }
                if (request.SyndicateCardPhoto is not null)
                {
                    syndicateKey = await UploadOptionalAsync(request.SyndicateCardPhoto, false);
                    if (syndicateKey is null)
                        throw new InvalidOperationException("Syndicate card upload did not return a storage key.");
                    uploadedKeys.Add(syndicateKey);
                }
                if (request.AvatarPhoto is not null)
                {
                    avatarKey = await UploadOptionalAsync(request.AvatarPhoto, true);
                    if (avatarKey is null)
                        throw new InvalidOperationException("Avatar upload did not return a storage key.");
                    uploadedKeys.Add(avatarKey);
                }
            }
            catch (Exception ex)
            {
                await CleanupUploadedKeysAsync(uploadedKeys);
                _logger?.LogError(ex, "Failed to upload identity documents during lawyer registration for email {Email}", request.Email);
                if (ex is OperationCanceledException)
                    throw;
                return Result<RegisterLawyerResponseDto>.Failure(
                    new Error("Upload.Failed", "Failed to upload required documents. Please try again."));
            }

            // 3. Build the Lawyer entity
            var lawyer = new Lawyer
            {
                FirstName               = request.FirstName,
                LastName                = request.LastName,
                FullName                = $"{request.FirstName} {request.LastName}",
                Email                   = request.Email,
                PasswordHash            = _authService.HashPassword(request.Password),
                CreatedAt               = DateTime.UtcNow,
                PhoneNo                 = request.PhoneNo,
                GovernorateId           = location.GovernorateId,
                Governorate             = location.Governorate,
                CityId                  = location.CityId,
                City                    = location.City,
                SyndicateId             = request.SyndicateId,
                YearsOfExperience       = request.YearsOfExperience,
                SSN_Url                 = ssnKey is null ? null : ssnKey,
                SyndicateCardUrl        = syndicateKey is null ? null : syndicateKey,
                AvatarUrl               = avatarKey is null || _storageService is null ? null : _storageService.GetPublicUrl(avatarKey),
                IsVerified              = false, // awaiting admin approval
                AccountStatus           = AlMostashar.Domain.ValueObject.Enum.AccountStatus.PendingReview,
                LawyerSpecializations   = selectedSpecializations ?? new List<Domain.Entities.LawyerSpecialization>()
            };

            // 3. Persist lawyer to DB — NO tokens generated
            //    A pending lawyer cannot use the app until admin verifies them.
            //    They will receive tokens when they Login after verification.
            try
            {
                await _db.Lawyers.AddAsync(lawyer, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await CleanupUploadedKeysAsync(uploadedKeys);
                _logger?.LogError(ex, "Failed to persist lawyer registration for email {Email}", request.Email);
                if (ex is OperationCanceledException)
                    throw;
                return Result<RegisterLawyerResponseDto>.Failure(
                    new Error("Lawyer.RegistrationFailed", "Failed to complete lawyer registration."));
            }

            // 4. Return pending response — mobile navigates to "pending review" screen
            return Result<RegisterLawyerResponseDto>.Success(new RegisterLawyerResponseDto
            {
                UserId             = lawyer.Id,
                Role               = AlMostashar.Domain.ValueObject.Enum.UserRole.Lawyer,
                AccountStatus      = AlMostashar.Domain.ValueObject.Enum.AccountStatus.PendingReview,
                ExpectedReviewDays = 2,
            });
        }

        private Task<string?> UploadOptionalAsync(Microsoft.AspNetCore.Http.IFormFile? file, bool isPublic)
        {
            if (file is null || _storageService is null)
                return Task.FromResult<string?>(null);

            return UploadOptionalAsyncCore(file, _storageService, isPublic);
        }

        private static async Task<string?> UploadOptionalAsyncCore(
            Microsoft.AspNetCore.Http.IFormFile file,
            IStorageService storageService,
            bool isPublic)
        {
            return isPublic
                ? await UploadToStorage.UploadPublicAsync(file, storageService)
                : await UploadToStorage.UploadAsync(file, storageService);
        }

        private async Task CleanupUploadedKeysAsync(IEnumerable<string> keys)
        {
            if (_storageService is null)
                return;

            foreach (var key in keys.Distinct(StringComparer.Ordinal))
            {
                try
                {
                    var result = await _storageService.DeleteFileAsync(key);
                    if (!result.Success)
                        _logger?.LogWarning("Failed to clean up uploaded registration file {StorageKey}: {Message}", key, result.Message);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to clean up uploaded registration file {StorageKey}", key);
                }
            }
        }
    }
}

