using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.LawyerHome.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.LawyerHome.Queries.GetLawyerActiveCasesSummary;

public class GetLawyerActiveCasesSummaryQueryHandler : IRequestHandler<GetLawyerActiveCasesSummaryQuery, Result<List<LawyerHomeActiveCaseDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetLawyerActiveCasesSummaryQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<LawyerHomeActiveCaseDto>>> Handle(GetLawyerActiveCasesSummaryQuery request, CancellationToken cancellationToken)
    {
        var lawyerId = _currentUser.UserId;

        // Fetch the recent active cases into memory to safely handle subtypes projection
        var recentCases = await _context.Cases
            .Where(c => c.LawyerId == lawyerId 
                     && (c.Status == CaseStatus.Open || c.Status == CaseStatus.InProgress))
            .OrderByDescending(c => c.CreatedAt)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        var dtos = recentCases.Select(c => new LawyerHomeActiveCaseDto
        {
            CaseId = c.Id,
            Title = c.Title,
            Status = c.Status.ToString(),
            ServiceType = c.ServiceType.ToString(),
            NextRelevantDate = GetNextRelevantDate(c),
            SecondaryText = GetSecondaryText(c)
        }).ToList();

        return Result<List<LawyerHomeActiveCaseDto>>.Success(dtos);
    }

    private static DateTime? GetNextRelevantDate(Case c)
    {
        return c switch
        {
            ConsultationCase cc => cc.AppointmentDate,
            LawsuitCase lc => lc.NextHearingDate,
            ContractCase ctc => ctc.DeliveryDate,
            _ => null
        };
    }

    private static string GetSecondaryText(Case c)
    {
        return c switch
        {
            ConsultationCase cc => cc.CommunicationMethod.ToString(),
            LawsuitCase lc => lc.CourtName,
            ContractCase ctc => ctc.ContractType.ToString(),
            CompanyFormationCase cfc => cfc.CompanyType.ToString(),
            _ => string.Empty
        };
    }
}
