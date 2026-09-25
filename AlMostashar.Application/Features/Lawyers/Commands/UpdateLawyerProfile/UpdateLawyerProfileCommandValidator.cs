using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Helpers;
using AlMostashar.Application.Common.Interfaces;
using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile;

public class UpdateLawyerProfileCommandValidator : AbstractValidator<UpdateLawyerProfileCommand>
{
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UpdateLawyerProfileCommandValidator(IAppDbContext db)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(Messages.Validation.FirstNameRequired)
            .MaximumLength(50).WithMessage(Messages.Validation.FirstNameMaxLength)
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(Messages.Validation.LastNameRequired)
            .MaximumLength(50).WithMessage(Messages.Validation.LastNameMaxLength)
            .When(x => x.LastName is not null);

        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0).WithMessage(Messages.Generic.Negative("YearsOfExperience"))
            .When(x => x.YearsOfExperience is not null);

        RuleFor(x => x.ProfileImage)
            .Must(file => file!.Length <= MaxImageSizeBytes)
            .WithMessage("Profile image must not exceed 5 MB.")
            .Must(file =>
            {
                var ext = Path.GetExtension(file!.FileName)?.ToLowerInvariant();
                return !string.IsNullOrEmpty(ext) && FileContentTypeHelper.AllowedImageExtensions.Contains(ext);
            })
            .WithMessage($"Profile image must be one of: {string.Join(", ", FileContentTypeHelper.AllowedImageExtensions)}.")
            .When(x => x.ProfileImage is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => x.Bio is not null);

        RuleFor(x => x.About)
            .MaximumLength(1000)
            .When(x => x.About is not null);

        RuleFor(x => x.SpecializationIds)
            .Must(ids => ids!.Distinct().Count() == ids!.Count)
            .WithMessage("Specialization IDs must be unique.")
            .When(x => x.SpecializationIds != null && x.SpecializationIds.Any());

        RuleFor(x => x.SpecializationIds)
            .MustAsync(async (ids, cancellation) =>
            {
                var existingCount = await db.LawyerSpecializations
                    .CountAsync(s => ids!.Contains(s.Id), cancellation);
                return existingCount == ids!.Count;
            })
            .WithMessage("One or more Specialization IDs do not exist.")
            .When(x => x.SpecializationIds != null && x.SpecializationIds.Any());
    }
}
