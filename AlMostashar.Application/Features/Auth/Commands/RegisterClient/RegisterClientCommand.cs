using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AlMostashar.Application.Features.Auth.Commands.RegisterClient
{
    public class RegisterClientCommand : IRequest<Result<RegisterClientResponseDto>>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        // Location selection for client
        public int GovernorateId { get; set; }
        public int CityId { get; set; }

        // Optional file uploads — use multipart/form-data
        public IFormFile? AvatarPhoto { get; set; }
        public IFormFile? FrontIdPhoto { get; set; }
        public IFormFile? BackIdPhoto { get; set; }
        // Clients do not upload syndicate membership cards
    }
}
