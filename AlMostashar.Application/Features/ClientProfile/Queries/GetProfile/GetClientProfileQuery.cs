using AlMostashar.Application.Features.ClientProfile.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientProfile.Queries.GetProfile;

public record GetClientProfileQuery : IRequest<Result<ClientProfileDto>>;
