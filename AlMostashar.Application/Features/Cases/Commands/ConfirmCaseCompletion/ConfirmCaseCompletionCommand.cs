using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.ConfirmCaseCompletion;

public record ConfirmCaseCompletionCommand(int CaseId) : IRequest<Result<string>>;
