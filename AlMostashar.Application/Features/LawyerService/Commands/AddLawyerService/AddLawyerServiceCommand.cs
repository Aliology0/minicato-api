using AlMostashar.Application.Features.LawyerService.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.LawyerService.Commands.AddLawyerService
{
    public class AddLawyerServiceCommand:IRequest<Result<LawyerServiceResponse>>
    {

        public int ServiceId { get; set; }
        public int Price { get; set; }
        public string Duration { get; set; } = null!;
    }
}
