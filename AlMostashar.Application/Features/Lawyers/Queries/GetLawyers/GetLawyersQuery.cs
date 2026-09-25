using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetLawyers
{
    public class GetLawyersQuery: IRequest<Result<CursorPagedResult<LawyerLookupDto>>>
    {
        public int? ServiceId { get; set; }
        public int? specializationId { get; set; }
        public int? Cursor { get; set; }
        public int PageSize { get; set; } = 10;
        public string? Search { get; set;}

    }
}
