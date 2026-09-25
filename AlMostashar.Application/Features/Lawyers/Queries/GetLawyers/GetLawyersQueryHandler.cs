using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetLawyers
{
    public class GetLawyersQueryHandler : IRequestHandler<GetLawyersQuery, Result<CursorPagedResult<LawyerLookupDto>>>
    {
        IAppDbContext _appDbContext;

        public GetLawyersQueryHandler(IAppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Result<CursorPagedResult<LawyerLookupDto>>> Handle(GetLawyersQuery request, CancellationToken cancellationToken)
        {
            var query = _appDbContext.Lawyers
                .Include(x=>x.LawyerServices)
                .Where(l => l.IsVerified == true && l.AccountStatus == AccountStatus.Active);

            if (request.ServiceId.HasValue)
            {
                query = query.Where(l => l.LawyerServices.Any(ls => ls.LegalServiceId == request.ServiceId.Value && ls.IsActive));
            }
            if (request.specializationId.HasValue)
            {
                query = query.Where(l => l.LawyerSpecializations!.Any(ls => ls.Id == request.specializationId));
            }
            if (request.Search is not null)
            {
                query = query.Where(l => l.FullName.Contains(request.Search));
            }

            query = query.OrderByDescending(l => l.Id);

            if (request.Cursor.HasValue)
            {
                query = query.Where(l => l.Id < request.Cursor.Value);
            }

            query = query.Take(request.PageSize + 1);

            List<LawyerLookupDto> result =await query.Select(x=>
            new LawyerLookupDto 
            {
                LawyerId = x.Id,
                FullName=x.FullName,
                ProfileImage= x.AvatarUrl??"",
                IsActive=x.IsActive,
                Rating= x.LawyerServices.SelectMany(x => x.Feedbacks.Select(x => x.Rate)).DefaultIfEmpty().Average()
            }).ToListAsync(cancellationToken) ?? new();
            bool HasMore= result.Count > request.PageSize;
            int? nextCursor = null;
            if (HasMore)
            {
                result.RemoveAt(result.Count-1);
                nextCursor = result[^1].LawyerId;
            }
            var pagedResult = new CursorPagedResult<LawyerLookupDto> { HasMore = HasMore, NextCursor = nextCursor, Items = result };
            return Result<CursorPagedResult<LawyerLookupDto>>.Success(pagedResult);
        }
    }
}
