using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AlMostashar.Application.Features.Cases.Services;

namespace AlMostashar.Application.Features.Cases.Commands.AddCase;

public class AddCaseCommandHandler : IRequestHandler<AddCaseCommand, Result<CaseDetailDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPublisher _publisher;
    private readonly ICaseMapper _caseMapper;

    public AddCaseCommandHandler(IAppDbContext db, ICurrentUserService currentUser, IPublisher publisher, ICaseMapper caseMapper)
    {
        _db = db;
        _currentUser = currentUser;
        _publisher = publisher;
        _caseMapper = caseMapper;
    }

    public async Task<Result<CaseDetailDto>> Handle(AddCaseCommand request, CancellationToken cancellationToken)
    {
        if (request.ClientRequestId.HasValue)
        {
            var existingCase = await GetCaseByClientRequestIdAsync(request.ClientRequestId.Value, cancellationToken);
            if (existingCase is not null)
                return Result<CaseDetailDto>.Success(MapToDto(existingCase));
        }

        var lawyerId = request.LawyerId ?? _currentUser.UserId;
        var lawyer = await _db.Lawyers.FirstOrDefaultAsync(l => l.Id == lawyerId, cancellationToken);

        var caseEntity = _caseMapper.Create(new CaseCreationData(
            request.ServiceType, lawyerId, request.Title, request.Description, request.ClientName,
            request.AppointmentDate, request.CommunicationMethod, request.LegalBranch,
            ContractType: request.ContractType, Language: request.Language,
            AllowedRevisions: request.AllowedRevisions, DeliveryDate: request.DeliveryDate,
            CompanyType: request.CompanyType, CapitalAmount: request.CapitalAmount,
            FoundersCount: request.FoundersCount, HasPowerOfAttorney: request.HasPowerOfAttorney,
            CourtName: request.CourtName, CaseNumber: request.CaseNumber,
            NextHearingDate: request.NextHearingDate, LawsuitStatus: request.LawsuitStatus,
            ClientRole: request.ClientRole));

        if (request.ClientRequestId.HasValue)
        {
            caseEntity.CaseClientRequest = new CaseClientRequest
            {
                Case = caseEntity,
                ClientRequestId = request.ClientRequestId.Value,
                CreatedAt = DateTime.UtcNow
            };

            _db.CaseClientRequests.Add(caseEntity.CaseClientRequest);
        }
        else
        {
            _db.Cases.Add(caseEntity);
        }

        await _db.SaveChangesAsync(cancellationToken);

        string? linkedClientName = null;
        int? linkedClientId = null;

        if (request.ClientRequestId.HasValue)
        {
            var clientRequestInfo = await _db.ClientRequests
                .Where(cr => cr.Id == request.ClientRequestId.Value)
                .Select(cr => new
                {
                    cr.ClientId,
                    cr.Client.FullName
                })
                .FirstAsync(cancellationToken);

            linkedClientId = clientRequestInfo.ClientId;
            linkedClientName = clientRequestInfo.FullName;
        }

        if (request.PublishCaseCreatedEvent && linkedClientId.HasValue)
        {
            await _publisher.Publish(
                new CaseCreatedEvent(caseEntity.Id, lawyerId, linkedClientId.Value),
                cancellationToken);
        }

        var dto = MapToDto(caseEntity, lawyer?.FullName, linkedClientName);
        return Result<CaseDetailDto>.Success(dto);
    }

    private async Task<Case?> GetCaseByClientRequestIdAsync(int clientRequestId, CancellationToken cancellationToken)
    {
        return await _db.Cases
            .Include(c => c.Lawyer)
            .Include(c => c.Chat)
            .Include(c => c.CaseClientRequest)
                .ThenInclude(ccr => ccr!.ClientRequest)
                    .ThenInclude(cr => cr.Client)
            .FirstOrDefaultAsync(
                c => c.CaseClientRequest != null && c.CaseClientRequest.ClientRequestId == clientRequestId,
                cancellationToken);
    }

    private static CaseDetailDto MapToDto(Case caseEntity, string? lawyerName = null, string? linkedClientName = null)
    {
        return new CaseDetailDto
        {
            Id = caseEntity.Id,
            Title = caseEntity.Title,
            Description = caseEntity.Description,
            ClientName = caseEntity.ClientName ?? linkedClientName ?? caseEntity.CaseClientRequest?.ClientRequest?.Client?.FullName,
            LawyerName = lawyerName ?? caseEntity.Lawyer?.FullName,
            LawyerId = caseEntity.LawyerId,
            ClientRequestId = caseEntity.CaseClientRequest?.ClientRequestId,
            ClientRequestReference = caseEntity.CaseClientRequest?.ClientRequest?.RequestId,
            ServiceId = caseEntity.CaseClientRequest?.ClientRequest?.LawyerServiceLegalServiceId,
            ChatId = caseEntity.Chat?.Id,
            CancellationReason = caseEntity.CancellationReason,
            ServiceType = caseEntity.ServiceType,
            Status = caseEntity.Status,
            CreatedAt = caseEntity.CreatedAt,
            Source = caseEntity.CaseClientRequest != null ? CaseSource.Platform : CaseSource.External,
            Reference = caseEntity.ReferenceNumber,
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
            ClientRole = (caseEntity as LawsuitCase)?.ClientRole
        };
    }
}
