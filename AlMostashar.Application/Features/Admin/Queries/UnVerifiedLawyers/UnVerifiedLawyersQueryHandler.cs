using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Admin.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Admin.Queries.UnVerifiedLawyers
{
    public class UnVerifiedLawyersQueryHandler : IRequestHandler<UnVerifiedLawyersQuery, Result<List<UnVerifiedLawyersDto>>>
    {
        private readonly IAppDbContext _dbContext;

        public UnVerifiedLawyersQueryHandler(IAppDbContext appDbContext)
        {
            _dbContext = appDbContext;
        }
        public async Task<Result<List<UnVerifiedLawyersDto>>> Handle(UnVerifiedLawyersQuery request, CancellationToken cancellationToken)
        {
            var unVerifiedLawyers= await _dbContext.Lawyers
                .Where(l=>l.AccountStatus == AlMostashar.Domain.ValueObject.Enum.AccountStatus.PendingReview)
                .Select(p=> 
                new UnVerifiedLawyersDto(p.Id,
                p.FullName,
                p.CreatedAt,
                p.PhoneNo,
                p.SyndicateId,
                p.SSN_Url,
                p.SyndicateCardUrl,
                p.PracticeCertificatesUrl)).ToListAsync(cancellationToken)?? new List<UnVerifiedLawyersDto>();
            return Result<List<UnVerifiedLawyersDto>>.Success(unVerifiedLawyers);
        }
    }
}
