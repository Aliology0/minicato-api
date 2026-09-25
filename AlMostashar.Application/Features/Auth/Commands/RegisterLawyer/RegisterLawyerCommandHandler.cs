using MediatR;
using Microsoft.EntityFrameworkCore;
using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Entities;

using AlMostashar.Domain.Shared;
using AlMostashar.Application.Common.Locations;

namespace AlMostashar.Application.Features.Auth.Commands.RegisterLawyer
{
    public class RegisterLawyerCommandHandler : IRequestHandler<RegisterLawyerCommand, Result<RegisterLawyerResponseDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IAuthService  _authService;
        private readonly ILocationCatalog _locationCatalog;
        public RegisterLawyerCommandHandler(IAppDbContext db, IAuthService authService, ILocationCatalog locationCatalog)
        {
            _db = db;
            _authService = authService;
            _locationCatalog = locationCatalog;
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

            bool syndicateExists = await _db.Lawyers
                .AnyAsync(l => l.SyndicateId == request.SyndicateId, cancellationToken);

            if (syndicateExists)
            {
                var error = new Error("User.Conflict", Messages.Auth.SyndicateIdAlreadyRegistered);
                return Result<RegisterLawyerResponseDto>.Failure(error);
            }

            // 2. Build the Lawyer entity
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
                AvatarUrl               = request.AvatarUrl,
                SSN_Url                 = request.SSN_Url,
                SyndicateCardUrl        = request.SyndicateCardUrl,
                PracticeCertificatesUrl = request.PracticeCertificatesUrl,
                IsVerified              = false, // awaiting admin approval
                AccountStatus           = AlMostashar.Domain.ValueObject.Enum.AccountStatus.PendingReview,
                VerificationStatus      = AlMostashar.Domain.ValueObject.Enum.VerificationStatus.Pending,
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

