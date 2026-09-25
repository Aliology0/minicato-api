using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(string Token) : IRequest<Result<AuthResponseDto>>;
}
