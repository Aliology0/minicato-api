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
        public string AvatarUrl { get; set; } = null!;
        public string SSN_Url { get; set; } = null!;
        public string SyndicateCardUrl { get; set; } = null!;
        public string? PracticeCertificatesUrl { get; set; }
    }
}
