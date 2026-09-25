using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Cases.Commands.UploadCaseDocument;

public class UploadCaseDocumentCommandValidator : AbstractValidator<UploadCaseDocumentCommand>
{
    public UploadCaseDocumentCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId(Messages.Fields.CaseId));

        RuleFor(x => x.DocumentId).GreaterThan(0);
    }
}
