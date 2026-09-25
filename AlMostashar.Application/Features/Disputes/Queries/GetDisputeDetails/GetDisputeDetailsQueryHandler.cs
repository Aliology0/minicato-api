using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Disputes.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Disputes.Queries.GetDisputeDetails;

public class GetDisputeDetailsQueryHandler : IRequestHandler<GetDisputeDetailsQuery, Result<DisputeDetailsDto>>
{
    private readonly IAppDbContext _context;

    public GetDisputeDetailsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DisputeDetailsDto>> Handle(GetDisputeDetailsQuery request, CancellationToken cancellationToken)
    {
        var dispute = await _context.Disputes
            .Include(d => d.OpenedByUser)
            .Include(d => d.ReviewedByAdmin)
            .Include(d => d.Case)
                .ThenInclude(c => c.Lawyer)
            .Include(d => d.Case)
                .ThenInclude(c => c.CaseClientRequest)
                    .ThenInclude(ccr => ccr!.ClientRequest)
                        .ThenInclude(cr => cr.Client)

            .Include(d => d.Escrow)
                .ThenInclude(e => e!.Payment)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

        if (dispute == null)
            return Result<DisputeDetailsDto>.Failure(new Error("Dispute.NotFound", Messages.Generic.NotFound("Dispute")));

        var client = dispute.Case?.CaseClientRequest?.ClientRequest?.Client;
        var lawyer = dispute.Case?.Lawyer;

        DisputePartyDto? initiator = null;
        DisputePartyDto? respondent = null;

        if (client != null && dispute.OpenedByUserId == client.Id)
        {
            initiator = new DisputePartyDto
            {
                Id = client.Id,
                Name = client.FullName,
                Avatar = client.AvatarUrl,
                Role = "العميل"
            };
            if (lawyer != null)
            {
                respondent = new DisputePartyDto
                {
                    Id = lawyer.Id,
                    Name = lawyer.FullName,
                    Avatar = lawyer.AvatarUrl,
                    Role = "المستشار"
                };
            }
        }
        else if (lawyer != null && dispute.OpenedByUserId == lawyer.Id)
        {
            initiator = new DisputePartyDto
            {
                Id = lawyer.Id,
                Name = lawyer.FullName,
                Avatar = lawyer.AvatarUrl,
                Role = "المستشار"
            };
            if (client != null)
            {
                respondent = new DisputePartyDto
                {
                    Id = client.Id,
                    Name = client.FullName,
                    Avatar = client.AvatarUrl,
                    Role = "العميل"
                };
            }
        }
        else
        {
            initiator = new DisputePartyDto
            {
                Id = dispute.OpenedByUserId,
                Name = dispute.OpenedByUser?.FullName ?? "Unknown",
                Avatar = dispute.OpenedByUser?.AvatarUrl,
                Role = "Unknown"
            };
        }

        DisputeFinancialsDto? financials = null;
        if (dispute.Escrow != null)
        {
            financials = new DisputeFinancialsDto
            {
                PaymentId = dispute.Escrow.PaymentId,
                EscrowAmount = dispute.Escrow.Amount,
                Amount = dispute.Escrow.Payment?.Amount ?? dispute.Escrow.Amount,
                Currency = dispute.Escrow.Payment?.Currency ?? "EGP",
                PaymentMethod = dispute.Escrow.Payment?.PaymentMethod
            };
        }

        var attachments = dispute.Attachments?.Select(a => new DisputeAttachmentDto
        {
            Name = a.Name,
            Size = a.Size,
            Path = a.Path
        }).ToList() ?? new List<DisputeAttachmentDto>();

        var disputeChatMessages = dispute.DisputeChatMessages?.Select(m => new DisputeChatMessageDto
        {
            SenderId = m.SenderId,
            Content = m.Content,
            SentAt = m.SentAt
        }).ToList() ?? new List<DisputeChatMessageDto>();

        return Result<DisputeDetailsDto>.Success(new DisputeDetailsDto
        {
            Id = dispute.Id,
            CaseId = dispute.CaseId,
            EscrowId = dispute.EscrowId,
            OpenedByUserId = dispute.OpenedByUserId,
            OpenedByUserName = dispute.OpenedByUser?.FullName?? "Unknown",
            Reason = dispute.Reason,
            Status = dispute.Status,
            Priority = dispute.Priority,
            AdminDecision = dispute.AdminDecision,
            AdminNotes = dispute.AdminNotes,
            CreatedAt = dispute.CreatedAt,
            ResolvedAt = dispute.ResolvedAt,
            ReviewedByAdminId = dispute.ReviewedByAdminId,
            ReviewedByAdminName = dispute.ReviewedByAdmin?.FullName,

            LastActivityAt = dispute.Case?.Chat?.LastMessageAt ?? dispute.CreatedAt,
            Initiator = initiator,
            Respondent = respondent,
            Financials = financials,
            Attachments = attachments,
            DisputeChatMessages = disputeChatMessages
        });
    }
}
