using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.StartCase;

public record StartCaseCommand(int CaseId) : IRequest<Result<string>>;
