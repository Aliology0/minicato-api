using MediatR;
using AlMostashar.Application.Features.Admin.DTOs;
using AlMostashar.Domain.Shared;

namespace AlMostashar.Application.Features.Admin.Commands.RegisterAdmin
{
    public class RegisterAdminCommand : IRequest<Result<RegisterAdminResponseDto>>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNo { get; set; } = null!;
    }
}
