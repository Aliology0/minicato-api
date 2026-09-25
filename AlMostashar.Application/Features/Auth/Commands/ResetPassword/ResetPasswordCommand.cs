using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<Result<string>>
    {
        public string ResetToken { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
