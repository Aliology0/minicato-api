using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Helpers;
using FluentValidation;
using System.IO;

namespace AlMostashar.Application.Features.Auth.Commands.RegisterClient
{
    public class RegisterClientCommandValidator : AbstractValidator<RegisterClientCommand>
    {
        public RegisterClientCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(Messages.Validation.FirstNameRequired)
                .MaximumLength(50).WithMessage(Messages.Validation.FirstNameMaxLength);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(Messages.Validation.LastNameRequired)
                .MaximumLength(50).WithMessage(Messages.Validation.LastNameMaxLength);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(Messages.Validation.EmailRequired)
                .EmailAddress().WithMessage(Messages.Validation.EmailInvalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Messages.Validation.PasswordRequired)
                .MinimumLength(6).WithMessage(Messages.Validation.PasswordMinLength)
                .Matches(@"[A-Z]").WithMessage(Messages.Validation.PasswordUppercase)
                .Matches(@"[a-z]").WithMessage(Messages.Validation.PasswordLowercase)
                .Matches(@"[0-9]").WithMessage(Messages.Validation.PasswordDigit);

            // File validations for optional uploads
            const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

            When(x => x.AvatarPhoto != null, () =>
            {
                RuleFor(x => x.AvatarPhoto!)
                    .Must(f => f.Length <= MaxFileSizeBytes).WithMessage(Messages.Validation.FileTooLarge)
                    .Must(f => {
                        var ext = Path.GetExtension(f.FileName)?.ToLowerInvariant();
                        return !string.IsNullOrEmpty(ext) && FileContentTypeHelper.AllowedExtensions.Contains(ext);
                    }).WithMessage(Messages.Validation.FileTypeNotAllowed);
            });

            When(x => x.FrontIdPhoto != null, () =>
            {
                RuleFor(x => x.FrontIdPhoto!)
                    .Must(f => f.Length <= MaxFileSizeBytes).WithMessage(Messages.Validation.FileTooLarge)
                    .Must(f => {
                        var ext = Path.GetExtension(f.FileName)?.ToLowerInvariant();
                        return !string.IsNullOrEmpty(ext) && FileContentTypeHelper.AllowedExtensions.Contains(ext);
                    }).WithMessage(Messages.Validation.FileTypeNotAllowed);
            });

            When(x => x.BackIdPhoto != null, () =>
            {
                RuleFor(x => x.BackIdPhoto!)
                    .Must(f => f.Length <= MaxFileSizeBytes).WithMessage(Messages.Validation.FileTooLarge)
                    .Must(f => {
                        var ext = Path.GetExtension(f.FileName)?.ToLowerInvariant();
                        return !string.IsNullOrEmpty(ext) && FileContentTypeHelper.AllowedExtensions.Contains(ext);
                    }).WithMessage(Messages.Validation.FileTypeNotAllowed);
            });

            When(x => x.SyndicateMembershipCardPhoto != null, () =>
            {
                RuleFor(x => x.SyndicateMembershipCardPhoto!)
                    .Must(f => f.Length <= MaxFileSizeBytes).WithMessage(Messages.Validation.FileTooLarge)
                    .Must(f => {
                        var ext = Path.GetExtension(f.FileName)?.ToLowerInvariant();
                        return !string.IsNullOrEmpty(ext) && FileContentTypeHelper.AllowedExtensions.Contains(ext);
                    }).WithMessage(Messages.Validation.FileTypeNotAllowed);
            });
        }
    }
}
