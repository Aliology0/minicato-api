using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<Result<string>>
    {
        public string Email { get; set; } = null!;
    }
}
