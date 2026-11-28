using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile;

public class UpdateLawyerProfileCommand : IRequest<Result<UpdateLawyerProfileResponseDto>>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public IFormFile? AvatarPhoto { get; set; }
    public IFormFile? SSNPhoto { get; set; }
    public IFormFile? SyndicateCardPhoto { get; set; }
    public string? PhoneNumber { get; set; }
    public int? GovernorateId { get; set; }
    public int? CityId { get; set; }
    public int? SyndicateId { get; set; }
    public string? Bio { get; set; }
    public string? About { get; set; }
    public int? YearsOfExperience { get; set; }
    public List<int>? SpecializationIds { get; set; }
    public bool ResubmitForReview { get; set; }
}
