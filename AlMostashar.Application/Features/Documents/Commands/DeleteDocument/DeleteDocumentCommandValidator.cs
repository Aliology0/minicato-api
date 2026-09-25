using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
    {
        public DeleteDocumentCommandValidator()
        {
            RuleFor(x => x.DocumentId)
                .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("DocumentId"));
        }
    }
}
