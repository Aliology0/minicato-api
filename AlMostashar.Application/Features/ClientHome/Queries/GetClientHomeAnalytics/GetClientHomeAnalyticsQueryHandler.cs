using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientHome.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientHome.Queries.GetClientHomeAnalytics;

public class GetClientHomeAnalyticsQueryHandler : IRequestHandler<GetClientHomeAnalyticsQuery, Result<ClientHomeAnalyticsDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetClientHomeAnalyticsQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ClientHomeAnalyticsDto>> Handle(GetClientHomeAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var clientId = _currentUser.UserId;

        var totalRequests = await _context.ClientRequests
            .CountAsync(r => r.ClientId == clientId, cancellationToken);

        var activeCasesCount = await _context.Cases
            .CountAsync(c => c.CaseClientRequest!.ClientRequest!.ClientId == clientId 
                          && (c.Status == CaseStatus.Open || c.Status == CaseStatus.InProgress), 
                cancellationToken);

        var completedCasesCount = await _context.Cases
            .CountAsync(c => c.CaseClientRequest!.ClientRequest!.ClientId == clientId 
                          && c.Status == CaseStatus.Closed, 
                cancellationToken);

        var pendingOffersCount = await _context.RequestOffers
            .CountAsync(o => o.ClientRequest!.ClientId == clientId 
                          && o.Status == OfferStatus.Pending, 
                cancellationToken);

        var unpaidInvoicesCount = await _context.Invoices
            .CountAsync(i => i.ClientRequest!.ClientId == clientId 
                          && i.Status == InvoiceStatus.Unpaid, 
                cancellationToken);

        var unreadMessagesCount = await _context.ChatParticipants
            .Where(cp => cp.UserId == clientId)
            .SumAsync(cp => cp.UnReadMessageCount, cancellationToken);

        var dto = new ClientHomeAnalyticsDto
        {
            TotalRequests = totalRequests,
            ActiveCasesCount = activeCasesCount,
            CompletedCasesCount = completedCasesCount,
            PendingOffersCount = pendingOffersCount,
            UnpaidInvoicesCount = unpaidInvoicesCount,
            UnreadMessagesCount = unreadMessagesCount
        };

        return Result<ClientHomeAnalyticsDto>.Success(dto);
    }
}
