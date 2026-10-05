using MediatR;
using Microsoft.EntityFrameworkCore;
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
        public RegisterLawyerCommandHandler(IAppDbContext db, IAuthService authService, ILocationCatalog locationCatalog, IStorageService? storageService = null)
    {
        _db = db;
        _authService = authService;
        _locationCatalog = locationCatalog;
        _storageService = storageService;
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

            // 2. If files provided, upload them and set URLs
            string? ssnKey = null;
            string? syndicateKey = null;
            string? practiceKey = null;
            try
            {
                var ssnTask = (request.SSNPhoto != null && _storageService != null)
                    ? UploadToStorage.UploadAsync(request.SSNPhoto, _storageService)
                    : Task.FromResult<string?>(null);

                var syndicateTask = (request.SyndicateCardPhoto != null && _storageService != null)
                    ? UploadToStorage.UploadAsync(request.SyndicateCardPhoto, _storageService)
                    : Task.FromResult<string?>(null);

                var practiceTask = (request.PracticeCertificatesPhoto != null && _storageService != null)
                    ? UploadToStorage.UploadAsync(request.PracticeCertificatesPhoto, _storageService)
                    : Task.FromResult<string?>(null);

                await Task.WhenAll(ssnTask, syndicateTask, practiceTask);

                ssnKey = await ssnTask;
                syndicateKey = await syndicateTask;
                practiceKey = await practiceTask;
            }
            catch
            {
                // Don't fail registration on storage issues; log elsewhere
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
                SSN_Url                 = ssnKey,
                SyndicateCardUrl        = syndicateKey,
                PracticeCertificatesUrl = practiceKey,
                IsVerified              = false, // awaiting admin approval
                AccountStatus           = AlMostashar.Domain.ValueObject.Enum.AccountStatus.PendingReview,
            };

            // 3. Persist lawyer to DB — NO tokens generated
            //    A pending lawyer cannot use the app until admin verifies them.
            //    They will receive tokens when they Login after verification.
            await _db.Lawyers.AddAsync(lawyer, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 4. Return pending response — mobile navigates to "pending review" screen
            return Result<RegisterLawyerResponseDto>.Success(new RegisterLawyerResponseDto
            {
                UserId             = lawyer.Id,
                Role               = AlMostashar.Domain.ValueObject.Enum.UserRole.Lawyer,
                AccountStatus      = AlMostashar.Domain.ValueObject.Enum.AccountStatus.PendingReview,
                ExpectedReviewDays = 2,
            });
        }
    }
}

