using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Helpers;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace AlMostashar.Application.Features.Auth.Commands.UploadIdentityDocuments
{
    public class UploadIdentityDocumentsCommandValidator : AbstractValidator<UploadIdentityDocumentsCommand>
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public UploadIdentityDocumentsCommandValidator()
        {
            // At least one file must be provided
            RuleFor(x => x)
                .Must(x => x.SSN is not null || x.SyndicateCard is not null || x.PracticeCertificates is not null)
                .WithMessage(Messages.Validation.AtLeastOneFileRequired);

            When(x => x.SSN is not null, () =>
            {
                RuleFor(x => x.SSN!)
                    .Must(BeValidSize).WithMessage(Messages.Validation.FileTooLarge)
                    .Must(BeAllowedExtension).WithMessage(Messages.Validation.FileTypeNotAllowed);
            });

            When(x => x.SyndicateCard is not null, () =>
            {
                RuleFor(x => x.SyndicateCard!)
                    .Must(BeValidSize).WithMessage(Messages.Validation.FileTooLarge)
                    .Must(BeAllowedExtension).WithMessage(Messages.Validation.FileTypeNotAllowed);
            });

            When(x => x.PracticeCertificates is not null, () =>
            {
                RuleFor(x => x.PracticeCertificates!)
                    .Must(BeValidSize).WithMessage(Messages.Validation.FileTooLarge)
                    .Must(BeAllowedExtension).WithMessage(Messages.Validation.FileTypeNotAllowed);
            });
        }

        private static bool BeValidSize(IFormFile file) => file.Length <= MaxFileSizeBytes;

        private static bool BeAllowedExtension(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension)
                && FileContentTypeHelper.AllowedExtensions.Contains(extension);
        }
    }
}
