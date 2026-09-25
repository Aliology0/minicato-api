using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Queries.GetMyCases;

public record GetMyCasesQuery(
    CaseStatus? Status,
    CaseSource? Source,
    ServiceType? ServiceType,
    string? Search,
    int? Cursor,
    int PageSize = 10
) : IRequest<Result<CursorPagedResult<CaseDto>>>;
