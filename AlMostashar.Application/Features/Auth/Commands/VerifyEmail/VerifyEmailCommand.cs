using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommand : IRequest<Result<AuthResponseDto>>
    {
        public int UserId { get; set; }
        public string OtpCode { get; set; } = null!;
    }
}

