using AlMostashar.Application.Common.Helpers;
using AlMostashar.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile;

public sealed class UpdateLawyerProfileCommandValidator : AbstractValidator<UpdateLawyerProfileCommand>
{
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    public UpdateLawyerProfileCommandValidator(IAppDbContext db)
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50).When(x => x.FirstName is not null);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50).When(x => x.LastName is not null);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 70).When(x => x.YearsOfExperience.HasValue);
        RuleFor(x => x.SyndicateId).GreaterThan(0).When(x => x.SyndicateId.HasValue);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]{7,15}$")
            .WithMessage("Phone number must contain 7 to 15 digits, optionally prefixed by +.")
            .When(x => x.PhoneNumber is not null);
        RuleFor(x => x.Bio).MaximumLength(1000).When(x => x.Bio is not null);
        RuleFor(x => x.About).MaximumLength(2000).When(x => x.About is not null);
        RuleFor(x => x)
            .Must(x => x.GovernorateId.HasValue == x.CityId.HasValue)
            .WithMessage("GovernorateId and CityId must be supplied together.");

        ValidateImage(x => x.AvatarPhoto, "Avatar");
        ValidateImage(x => x.SSNPhoto, "SSN");
        ValidateImage(x => x.SyndicateCardPhoto, "Syndicate card");

        RuleFor(x => x.SpecializationIds)
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
            .WithMessage("Specialization IDs must be unique.");
        RuleFor(x => x.SpecializationIds)
            .MustAsync(async (ids, cancellation) => ids is null ||
                await db.LawyerSpecializations.CountAsync(s => ids.Contains(s.Id) && s.IsActive, cancellation) == ids.Count)
            .WithMessage("One or more specialization IDs do not exist or are inactive.");
    }

    private void ValidateImage(System.Linq.Expressions.Expression<Func<UpdateLawyerProfileCommand, IFormFile?>> selector, string label)
    {
        RuleFor(selector)
            .Must(file => file is null || (file.Length > 0 && file.Length <= MaxImageSizeBytes))
            .WithMessage($"{label} image must be non-empty and not exceed 5 MB.")
            .Must(file => file is null || FileContentTypeHelper.AllowedImageExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            .WithMessage($"{label} image type is not supported.");
    }
}
