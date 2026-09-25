using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Disputes.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.Disputes.Queries.GetDisputesList;

public class GetDisputesListQuery : IRequest<Result<CursorPagedResult<DisputeListItemDto>>>
{
    public DisputeStatus? Status { get; set; }
    public int PageSize { get; set; } = 20;
    public int? Cursor { get; set; }
    public DateTime? CursorDate { get; set; }
}
