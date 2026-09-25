using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.LegalService.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.LegalService.Queries.GetLegalServices
{
    public class GetLegalServicesQueryHandler : IRequestHandler<GetLegalServicesQuery, Result<List<LegalServiceResponse>>>
    {
        private readonly IAppDbContext _appDbContext;

        public GetLegalServicesQueryHandler(IAppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Result<List<LegalServiceResponse>>> Handle(GetLegalServicesQuery request, CancellationToken cancellationToken)
        {
            var services = await _appDbContext.LegalServices
                .AsNoTracking()
                .Select(x => new LegalServiceResponse(
                    x.Id,
                    x.Title,
                    x.Summary,
                    x.FullDescription,
                    x.ServiceType.ToString(),
                    x.IconUrl,
                    x.RequiredDocuments,
                    x.ExpectedDuration,
                    x.IsActive,
                    x.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return Result<List<LegalServiceResponse>>.Success(services);
        }
    }
}
