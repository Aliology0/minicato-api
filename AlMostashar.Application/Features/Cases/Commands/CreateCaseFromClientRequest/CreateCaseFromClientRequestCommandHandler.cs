using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Cases.Commands.CreateCaseFromClientRequest;

public sealed class CreateCaseFromClientRequestCommandHandler
    : IRequestHandler<CreateCaseFromClientRequestCommand, Result<int>>
{
    private readonly IAppDbContext _db;
    private readonly IRequestDetailsService _requestDetailsService;
    private readonly IClientRequestCaseMapper _caseMapper;
    private readonly IPublisher _publisher;

    public CreateCaseFromClientRequestCommandHandler(
        IAppDbContext db,
        IRequestDetailsService requestDetailsService,
        IClientRequestCaseMapper caseMapper,
        IPublisher publisher)
    {
        _db = db;
        _requestDetailsService = requestDetailsService;
        _caseMapper = caseMapper;
        _publisher = publisher;
    }

    public async Task<Result<int>> Handle(CreateCaseFromClientRequestCommand command, CancellationToken cancellationToken)
    {
        var existingCaseId = await _db.CaseClientRequests
            .Where(link => link.ClientRequestId == command.ClientRequestId)
            .Select(link => (int?)link.CaseId)
            .SingleOrDefaultAsync(cancellationToken);
        if (existingCaseId.HasValue)
            return Result<int>.Success(existingCaseId.Value);

        var request = await _db.ClientRequests
            .Include(value => value.Client)
            .Include(value => value.RequestedLegalService)
            .Include(value => value.Documents)
            .Include(value => value.CaseClientRequest)
            .Include(value => value.ConsultationDetails)
            .Include(value => value.ContractDetails)
            .Include(value => value.LawsuitDetails)
            .Include(value => value.CompanyFormationDetails)
            .Include(value => value.GenericDetails)
            .SingleOrDefaultAsync(value => value.Id == command.ClientRequestId, cancellationToken);

        if (request is null)
            return Result<int>.Failure(new Error("Request.NotFound", "Client request was not found."));

        var lawyerId = request.LawyerServiceLawyerId;
        if (!lawyerId.HasValue)
            return Result<int>.Failure(new Error("Case.MissingLawyer", "The request has no accepted lawyer."));

        if (request.ServiceType == ServiceType.Base && request.RequestedLegalService is not null)
            request.ServiceType = request.RequestedLegalService.ServiceType;

        var details = _requestDetailsService.MapFromEntity(request);
        if (details is null)
            return Result<int>.Failure(new Error("Case.MissingRequestDetails", "Typed request details were not found."));

        var caseResult = _caseMapper.Create(request, lawyerId.Value, details);
        if (!caseResult.IsSuccess)
            return Result<int>.Failure(caseResult.Error!);

        var caseEntity = caseResult.Value!;
        caseEntity.CaseClientRequest = new CaseClientRequest
        {
            Case = caseEntity,
            ClientRequest = request,
            ClientRequestId = request.Id,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var document in request.Documents)
            document.Case = caseEntity;

        _db.CaseClientRequests.Add(caseEntity.CaseClientRequest);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var concurrentCaseId = await _db.CaseClientRequests
                .AsNoTracking()
                .Where(link => link.ClientRequestId == command.ClientRequestId)
                .Select(link => (int?)link.CaseId)
                .SingleOrDefaultAsync(cancellationToken);
            if (concurrentCaseId.HasValue)
                return Result<int>.Success(concurrentCaseId.Value);
            throw;
        }

        await _publisher.Publish(
            new CaseCreatedEvent(caseEntity.Id, lawyerId.Value, request.ClientId),
            cancellationToken);

        return Result<int>.Success(caseEntity.Id);
    }
}
