using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Commands.RejectRequest;

public record RejectRequestCommand(int RequestId) : IRequest<Result<string>>;
