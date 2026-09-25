using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.DeleteCaseNote;

public record DeleteCaseNoteCommand(
    int CaseId,
    int NoteId
) : IRequest<Result<string>>;
