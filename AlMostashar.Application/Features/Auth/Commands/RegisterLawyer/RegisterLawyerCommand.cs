using MediatR;
using AlMostashar.Application.Features.Auth.DTOs;

using AlMostashar.Domain.Shared;

namespace AlMostashar.Application.Features.Auth.Commands.RegisterLawyer
{
    public class RegisterLawyerCommand : IRequest<Result<RegisterLawyerResponseDto>>
    {
        // ── Common User fields ──────────────────────────────────────────────
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNo { get; set; } = null!;
        public int GovernorateId { get; set; }
        public int CityId { get; set; }
        // ── Lawyer-specific fields ──────────────────────────────────────────
        public int SyndicateId { get; set; }
        // File uploads (multipart/form-data)
        public Microsoft.AspNetCore.Http.IFormFile? SSNPhoto { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? SyndicateCardPhoto { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? PracticeCertificatesPhoto { get; set; }
    }
}
