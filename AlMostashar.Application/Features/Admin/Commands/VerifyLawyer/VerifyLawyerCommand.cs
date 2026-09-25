using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Admin.Commands.VerifyLawyer
{
    public class VerifyLawyerCommand : IRequest<Result<string>>
    {
        /// <summary>The ID of the lawyer to verify.</summary>
        public int LawyerId { get; set; }
        public bool IsAccepted { get; set; }

    }
}
