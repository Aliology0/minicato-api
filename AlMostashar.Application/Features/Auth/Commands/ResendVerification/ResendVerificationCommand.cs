using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Auth.Commands.ResendVerification
{
    public class ResendVerificationCommand : IRequest<Result<string>>
    {
        public string Email { get; set; } = null!;
    }
}
