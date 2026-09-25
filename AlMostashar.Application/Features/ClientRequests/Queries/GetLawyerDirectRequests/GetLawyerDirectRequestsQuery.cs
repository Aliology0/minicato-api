using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetLawyerDirectRequests;

public record GetLawyerDirectRequestsQuery(
    ClientRequestStatus? Status = null,
    ServiceType? ServiceType = null)
    : IRequest<Result<List<LawyerDirectRequestDto>>>;
