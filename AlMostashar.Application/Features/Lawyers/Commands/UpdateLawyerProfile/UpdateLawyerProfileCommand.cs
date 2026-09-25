using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile;

public class UpdateLawyerProfileCommand : IRequest<Result<UpdateLawyerProfileResponseDto>>
{
    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;
    public IFormFile? ProfileImage { get; set; }
    public string? Bio { get; set; }
    public string? About { get; set; }
    public int? YearsOfExperience { get; set; }
    public List<int>? SpecializationIds { get; set; }
}
