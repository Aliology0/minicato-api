using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.CreateCaseFromClientRequest;

public sealed record CreateCaseFromClientRequestCommand(int ClientRequestId) : IRequest<Result<int>>;
