using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.CancelCase;

public record CancelCaseCommand(int CaseId, string Reason) : IRequest<Result<string>>;
