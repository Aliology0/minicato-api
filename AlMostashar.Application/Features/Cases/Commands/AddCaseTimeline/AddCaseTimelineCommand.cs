using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.AddCaseTimeline;

public record AddCaseTimelineCommand(
    int CaseId,
    string Title,
    string Content
) : IRequest<Result<CaseTimelineDto>>;
