using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Commands.CancelRequest;

public record CancelRequestCommand(int RequestId) : IRequest<Result<string>>;
