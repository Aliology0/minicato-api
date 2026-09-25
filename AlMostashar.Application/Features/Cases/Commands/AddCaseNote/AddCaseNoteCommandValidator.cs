using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Cases.Commands.AddCaseNote;

public class AddCaseNoteCommandValidator : AbstractValidator<AddCaseNoteCommand>
{
    public AddCaseNoteCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId(Messages.Fields.CaseId));

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage(Messages.Generic.Required(Messages.Fields.NoteContent))
            .MaximumLength(5000)
            .WithMessage(Messages.Generic.MaxLength(Messages.Fields.NoteContent, 5000));
    }
}
