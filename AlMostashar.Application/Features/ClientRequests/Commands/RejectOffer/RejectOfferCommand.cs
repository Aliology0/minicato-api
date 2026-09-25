using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Commands.RejectOffer;

public record RejectOfferCommand(int OfferId) : IRequest<Result<string>>;
