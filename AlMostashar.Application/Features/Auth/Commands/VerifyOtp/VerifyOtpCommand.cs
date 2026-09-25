using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Auth.Commands.VerifyOtp
{
    public class VerifyOtpCommand : IRequest<Result<VerifyOtpResponseDto>>
    {
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
    }
}
