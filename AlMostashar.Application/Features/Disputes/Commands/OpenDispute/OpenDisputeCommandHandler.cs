using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static AlMostashar.Application.Helpers.UploadToStorage;

namespace AlMostashar.Application.Features.Disputes.Commands.OpenDispute;

public class OpenDisputeCommandHandler : IRequestHandler<OpenDisputeCommand, Result<int>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storageService;

    public OpenDisputeCommandHandler(IAppDbContext context, ICurrentUserService currentUser, IStorageService storageService)
    {
        _context = context;
        _currentUser = currentUser;
        _storageService = storageService;
    }

    public async Task<Result<int>> Handle(OpenDisputeCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var caseEntity = await _context.Cases
            .Include(c => c.CaseClientRequest).ThenInclude(ccr => ccr.ClientRequest)
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (caseEntity == null)
            return Result<int>.Failure(new Error("Case.NotFound", Messages.Generic.NotFound("Case")));

        bool isParticipant = caseEntity.LawyerId == currentUserId || 
                             (caseEntity.CaseClientRequest?.ClientRequest?.ClientId == currentUserId);

        if (!isParticipant)
            return Result<int>.Failure(new Error("Dispute.Unauthorized", "You must be a participant in the case to open a dispute."));

        // Automatically fetch and validate the Escrow associated with this case's request
        int? escrowId = null;
        if (caseEntity.CaseClientRequest != null)
        {
            var escrow = await _context.Escrows
                .FirstOrDefaultAsync(e => e.RequestId == caseEntity.CaseClientRequest.ClientRequestId, cancellationToken);
            
            if (escrow != null)
            {
                if (escrow.Status != EscrowStatus.Funded)
                {
                    return Result<int>.Failure(new Error("Escrow.InvalidStatus", $"Only funded escrows can be marked as disputed. Current status is {escrow.Status}."));
                }

                escrowId = escrow.Id;
                escrow.Status = EscrowStatus.Disputed;
                escrow.EscrowDisputedAt = DateTime.UtcNow;
                caseEntity.Status = CaseStatus.OnHold;
            }
        }

        var uploadedAttachments = new List<DisputeAttachment>();
        if (request.Attachments != null && request.Attachments.Any())
        {
            foreach (var file in request.Attachments)
            {
                var fileKey = await UploadAsync(file, _storageService);
                uploadedAttachments.Add(new DisputeAttachment
                {
                    Name = Path.GetFileName(file.FileName),
                    Size = file.Length,
                    Path = fileKey
                });
            }
        }

        var dispute = new Dispute
        {
            CaseId = request.CaseId,
            EscrowId = escrowId,
            OpenedByUserId = currentUserId,
            Reason = request.Reason,
            Attachments = uploadedAttachments,
            DisputeChatMessages = request.DisputeChatMessages?.Select(m => new DisputeChatMessage
            {
                SenderId = m.SenderId,
                Content = m.Content,
                SentAt = m.SentAt
            }).ToList() ?? new List<DisputeChatMessage>(),
            Status = DisputeStatus.Open,
            Priority = DisputePriority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        _context.Disputes.Add(dispute);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(dispute.Id);
    }
}
