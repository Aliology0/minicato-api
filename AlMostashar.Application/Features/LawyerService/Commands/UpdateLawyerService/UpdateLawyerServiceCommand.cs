using AlMostashar.Application.Features.LawyerService.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.LawyerService.Commands.UpdateLawyerService
{
    public class UpdateLawyerServiceCommand : IRequest<Result<LawyerServiceResponse>>
    {
        [JsonIgnore]
        public int ServiceId { get; set; }
        public decimal? Price { get; set; }
        public string? Duration { get; set; }
        public bool? IsActive { get; set; }
    }
}
