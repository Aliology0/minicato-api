using FluentValidation;

namespace AlMostashar.Application.Features.Payments.Commands.WebHook
{
    public class WebHookCommandValidator : AbstractValidator<WebHookCommand>
    {
        public WebHookCommandValidator()
        {
            RuleFor(x => x.Hmac).NotEmpty().WithMessage("HMAC is required");
            RuleFor(x => x.Payload).NotNull().WithMessage("Payload is required");
            RuleFor(x => x.Payload.obj).NotNull().When(x => x.Payload != null).WithMessage("Transaction object is required");
        }
    }
}
