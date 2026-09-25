using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.AddCaseNote;

public record AddCaseNoteCommand(
    int CaseId,
    string Content
) : IRequest<Result<CaseNoteDto>>;
