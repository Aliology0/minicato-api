using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Auth.Commands.RegisterClient
{
    public class RegisterClientCommand : IRequest<Result<RegisterClientResponseDto>>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string AvatarUrl { get; set; } = null!;
        public string NationalIdPhotoUrl { get; set; } = null!;
    }
}
