using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Commands.AcceptRequest;

public record AcceptRequestCommand(int RequestId)
    : IRequest<Result<string>>, ITransactionalCommand;
