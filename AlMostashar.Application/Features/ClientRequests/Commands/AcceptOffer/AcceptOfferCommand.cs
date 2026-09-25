using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Commands.AcceptOffer;

public record AcceptOfferCommand(int OfferId)
    : IRequest<Result<AcceptOfferResponseDto>>, ITransactionalCommand;
