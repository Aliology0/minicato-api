using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.LawyerService.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.LawyerService.Queries.GetLawyerServices
{
    public class GetLawyerServicesQueryHandler : IRequestHandler<GetLawyerServicesQuery, Result<List<LawyerServiceResponse>>>
    {
        private readonly IAppDbContext appDbContext;
        private readonly ICurrentUserService currentUserService;

        public GetLawyerServicesQueryHandler(IAppDbContext appDbContext, ICurrentUserService currentUserService)
        {
            this.appDbContext = appDbContext;
            this.currentUserService = currentUserService;
        }
        public async Task<Result<List<LawyerServiceResponse>>> Handle(GetLawyerServicesQuery request, CancellationToken cancellationToken)
        {
            int lawyerId = currentUserService.UserId;
            var lawyerService= await appDbContext.LawyerServices
                .Include(x => x.LegalService)
                .AsNoTracking()
                .Where(l => l.LawyerId == lawyerId)
                .Select(x=> new LawyerServiceResponse(
                    x.LegalServiceId,
                    x.LegalService.Title,
                    x.LegalService.Summary,
                    x.Price,
                    x.Duration,
                    x.IsActive
                    )).ToListAsync(cancellationToken)?? new List<LawyerServiceResponse>();
            return Result<List<LawyerServiceResponse>>.Success(lawyerService);
        }
    }
}
