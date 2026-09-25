using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AlMostashar.Application.Features.ClientProfile.Commands.EditProfile;

public class EditClientProfileCommand : IRequest<Result<Unit>>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public IFormFile? ProfileImage { get; set; }
}
