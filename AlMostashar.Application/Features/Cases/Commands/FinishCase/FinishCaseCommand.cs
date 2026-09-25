using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.FinishCase;

public record FinishCaseCommand(int CaseId) : IRequest<Result<string>>;
