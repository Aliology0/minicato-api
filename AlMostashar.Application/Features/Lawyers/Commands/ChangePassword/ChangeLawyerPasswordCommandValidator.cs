using FluentValidation;

namespace AlMostashar.Application.Features.Lawyers.Commands.ChangePassword;

public sealed class ChangeLawyerPasswordCommandValidator : AbstractValidator<ChangeLawyerPasswordCommand>
{
    public ChangeLawyerPasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("New password and confirmation do not match.");
    }
}
