using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Helpers;
using FluentValidation;

namespace AlMostashar.Application.Features.ClientProfile.Commands.EditProfile;

public class EditClientProfileCommandValidator : AbstractValidator<EditClientProfileCommand>
{
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    public EditClientProfileCommandValidator()
    {
        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage(Messages.Validation.FirstNameRequired)
            .MaximumLength(50).WithMessage(Messages.Validation.FirstNameMaxLength).When(v => v.FirstName is not null);

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage(Messages.Validation.LastNameRequired)
            .MaximumLength(50).WithMessage(Messages.Validation.LastNameMaxLength).When(v => v.LastName is not null);
            
        RuleFor(v => v.ProfileImage)
            .Must(file => file!.Length <= MaxImageSizeBytes)
            .WithMessage("Profile image must not exceed 5 MB.")
            .Must(file =>
            {
                var ext = Path.GetExtension(file!.FileName)?.ToLowerInvariant();
                return !string.IsNullOrEmpty(ext) && FileContentTypeHelper.AllowedImageExtensions.Contains(ext);
            })
            .WithMessage($"Profile image must be one of: {string.Join(", ", FileContentTypeHelper.AllowedImageExtensions)}.")
            .When(v => v.ProfileImage is not null);
    }
}
