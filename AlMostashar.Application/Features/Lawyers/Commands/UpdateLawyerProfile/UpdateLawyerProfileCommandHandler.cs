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

        // 2. Upload profile image if provided
        if (request.ProfileImage is not null)
        {
            var fileKey = await UploadPublicAsync(request.ProfileImage, _storageService);
            lawyer.AvatarUrl = _storageService.GetPublicUrl(fileKey);
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

        // 4. Save
        await _db.SaveChangesAsync(cancellationToken);

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
