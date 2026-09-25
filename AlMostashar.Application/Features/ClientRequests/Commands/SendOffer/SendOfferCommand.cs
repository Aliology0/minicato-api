using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Commands.SendOffer;

public record SendOfferCommand(
    int RequestId,
    int? LegalServiceId,
    decimal OfferedAmount,
    string? Note
) : IRequest<Result<RequestOfferDto>>;
