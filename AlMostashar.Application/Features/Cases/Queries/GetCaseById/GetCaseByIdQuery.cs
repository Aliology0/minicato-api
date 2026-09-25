using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Queries.GetCaseById;

public record GetCaseByIdQuery(
    int CaseId
) : IRequest<Result<CaseDetailDto>>;
