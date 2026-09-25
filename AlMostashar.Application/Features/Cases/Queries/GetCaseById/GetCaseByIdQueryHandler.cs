using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Cases.Queries.GetCaseById;

public class GetCaseByIdQueryHandler : IRequestHandler<GetCaseByIdQuery, Result<CaseDetailDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCaseByIdQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CaseDetailDto>> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
    {
        var requesterId = _currentUser.UserId;

        var caseEntity = await _db.Cases
            .Include(c => c.Notes)
            .Include(c => c.Timelines)
            .Include(c => c.Documents)
            .Include(c => c.Lawyer)
            .Include(c => c.Chat)
            .Include(c => c.CaseClientRequest)
                .ThenInclude(ccr => ccr!.ClientRequest)
                    .ThenInclude(cr => cr.Client)
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (caseEntity is null)
            return Result<CaseDetailDto>.Failure(new Error("Case.NotFound", Messages.Cases.NotFound));

        // Authorization: must be the lawyer or the linked client
        var isLawyer = caseEntity.LawyerId == requesterId;
        var isClient = caseEntity.CaseClientRequest?.ClientRequest?.ClientId == requesterId;

        if (!isLawyer && !isClient)
            return Result<CaseDetailDto>.Failure(new Error("Auth.Forbidden", Messages.Auth.CannotViewCase));

        var dto = new CaseDetailDto
        {
            Id = caseEntity.Id,
            Title = caseEntity.Title,
            Description = caseEntity.Description,
            ServiceType = caseEntity.ServiceType,
            Status = caseEntity.Status,
            CreatedAt = caseEntity.CreatedAt,
            Source = caseEntity.CaseClientRequest != null ? CaseSource.Platform : CaseSource.External,
            ClientName = caseEntity.ClientName ?? (caseEntity.CaseClientRequest?.ClientRequest?.Client?.FullName),
            LawyerName = caseEntity.Lawyer?.FullName,
            Reference = caseEntity.ReferenceNumber,
            LawyerId = caseEntity.LawyerId,
            ClientRequestId = caseEntity.CaseClientRequest?.ClientRequestId,
            ClientRequestReference = caseEntity.CaseClientRequest?.ClientRequest?.RequestId,
            ServiceId = caseEntity.CaseClientRequest?.ClientRequest?.LawyerServiceLegalServiceId,
            ChatId = caseEntity.Chat?.Id,
            CancellationReason = caseEntity.CancellationReason,

            // Subtype fields
            AppointmentDate = (caseEntity as ConsultationCase)?.AppointmentDate,
            CommunicationMethod = (caseEntity as ConsultationCase)?.CommunicationMethod,
            LegalBranch = (caseEntity as ConsultationCase)?.LegalBranch,
            ContractType = (caseEntity as ContractCase)?.ContractType,
            Language = (caseEntity as ContractCase)?.Language,
            AllowedRevisions = (caseEntity as ContractCase)?.AllowedRevisions,
            DeliveryDate = (caseEntity as ContractCase)?.DeliveryDate,
            CompanyType = (caseEntity as CompanyFormationCase)?.CompanyType,
            CapitalAmount = (caseEntity as CompanyFormationCase)?.CapitalAmount,
            FoundersCount = (caseEntity as CompanyFormationCase)?.FoundersCount,
            HasPowerOfAttorney = (caseEntity as CompanyFormationCase)?.HasPowerOfAttorney,
            CourtName = (caseEntity as LawsuitCase)?.CourtName,
            CaseNumber = (caseEntity as LawsuitCase)?.CaseNumber,
            NextHearingDate = (caseEntity as LawsuitCase)?.NextHearingDate,
            LawsuitStatus = (caseEntity as LawsuitCase)?.LawsuitStatus,
            ClientRole = (caseEntity as LawsuitCase)?.ClientRole,

            // Collections
            Notes = caseEntity.Notes.OrderByDescending(n => n.CreatedAt).Select(n => new CaseNoteDto
            {
                Id = n.Id,
                Content = n.Content,
                CreatedAt = n.CreatedAt
            }).ToList(),

            Timelines = caseEntity.Timelines.OrderByDescending(t => t.CreatedAt).Select(t => new CaseTimelineDto
            {
                Id = t.Id,
                Title = t.Title,
                Content = t.Content,
                CreatedAt = t.CreatedAt
            }).ToList(),

            Documents = caseEntity.Documents.OrderByDescending(d => d.CreatedAt).Select(d => new CaseDocumentDto
            {
                Id = d.Id,
                DocumentName = d.DocumentName,
                CreatedAt = d.CreatedAt
            }).ToList()
        };

        return Result<CaseDetailDto>.Success(dto);
    }
}
