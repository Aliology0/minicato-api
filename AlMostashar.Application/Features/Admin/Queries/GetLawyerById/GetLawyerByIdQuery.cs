using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Admin.Queries.GetLawyerById;

public sealed record GetLawyerByIdQuery(int Id) : IRequest<Result<AlMostashar.Application.Features.Admin.DTOs.LawyerDetailsDto>>;
