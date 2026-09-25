using AlMostashar.Application.Features.Disputes.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Disputes.Queries.GetDisputeDetails;

public record GetDisputeDetailsQuery(int DisputeId) : IRequest<Result<DisputeDetailsDto>>;
