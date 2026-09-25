using AlMostashar.Application.Features.Auth.DTOs;
using MediatR;

using AlMostashar.Domain.Shared;

namespace AlMostashar.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<Result<AuthResponseDto>>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
