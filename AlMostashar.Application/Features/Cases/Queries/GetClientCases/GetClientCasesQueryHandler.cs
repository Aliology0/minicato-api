using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Cases.Queries.GetClientCases;

public class GetClientCasesQueryHandler : IRequestHandler<GetClientCasesQuery, Result<List<ClientCasesDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetClientCasesQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ClientCasesDto>>> Handle(GetClientCasesQuery request, CancellationToken cancellationToken)
    {
        var clientRequests = await _db.CaseClientRequests
            .Include(ccr => ccr.ClientRequest)
            .Include(ccr => ccr.Case)
                .ThenInclude(c => c.Lawyer)
            .Include(ccr => ccr.Case)
                .ThenInclude(c => c.Documents)
            .Include(ccr => ccr.Case)
                .ThenInclude(c => c.Timelines)
            .Include(ccr => ccr.Case)
                .ThenInclude(c => c.Chat)
            .Where(ccr => ccr.ClientRequest.ClientId == _currentUser.UserId)
            .OrderByDescending(ccr => ccr.Case.CreatedAt)
            .ToListAsync(cancellationToken);

        var cases = clientRequests.Select(ccr => new ClientCasesDto
        {
            CaseId = ccr.Case.Id,
            ClientRequestId = ccr.ClientRequest.RequestId,
            ClientRequestNumericId = ccr.ClientRequestId,
            Reference = ccr.Case.ReferenceNumber,
            CaseTitle = ccr.Case.Title,
            Description = ccr.Case.Description,
            CaseNo = ccr.Case is LawsuitCase lc ? lc.CaseNumber : ccr.Case.Id.ToString(),
            CreatedAt = ccr.Case.CreatedAt,
            Status = ccr.Case.Status,
            LawyerId = ccr.Case.LawyerId,
            LawyerName = ccr.Case.Lawyer?.FullName ?? string.Empty,
            ServiceType = ccr.Case.ServiceType,
            ServiceId = ccr.ClientRequest.LawyerServiceLegalServiceId,
            ChatId = ccr.Case.Chat?.Id,
            CancellationReason = ccr.Case.CancellationReason,
            Docs = ccr.Case.Documents.Select(d => new CaseDocumentDto 
            {
                Id = d.Id,
                DocumentName = d.DocumentName,
                CreatedAt = d.CreatedAt
            }).ToList(),
            Timeline = ccr.Case.Timelines.Select(t => new CaseTimelineDto
            {
                Id = t.Id,
                Title = t.Title,
                Content = t.Content,
                CreatedAt = t.CreatedAt
            }).ToList()
        }).ToList();

        return Result<List<ClientCasesDto>>.Success(cases);
    }
}
