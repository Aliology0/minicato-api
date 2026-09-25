using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetLawyerHomeAnalytics;

public class GetLawyerHomeAnalyticsQueryHandler
    : IRequestHandler<GetLawyerHomeAnalyticsQuery, Result<LawyerAnalyticsDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetLawyerHomeAnalyticsQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<LawyerAnalyticsDto>> Handle(
        GetLawyerHomeAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var lawyerId = _currentUser.UserId;

        // Verify lawyer exists and is active
        var lawyerExists = await _db.Lawyers
            .AsNoTracking()
            .AnyAsync(l => l.Id == lawyerId
                        && l.AccountStatus == AccountStatus.Active
                        && l.IsVerified, cancellationToken);

        if (!lawyerExists)
        {
            return Result<LawyerAnalyticsDto>.Failure(
                new Error("Lawyer.NotFound", "Lawyer not found or profile is not active."));
        }

        // Ratings logic mapping to existing GetLawyerProfile handler
        var ratingSum = await _db.Feedbacks
            .Where(f => f.ClientRequest.LawyerServiceLawyerId == lawyerId)
            .Select(f => (double?)f.Rate)
            .SumAsync(cancellationToken) ?? 0.0;

        var ratingsCount = await _db.Feedbacks
            .CountAsync(f => f.ClientRequest.LawyerServiceLawyerId == lawyerId, cancellationToken);

        var ratingAverage = ratingsCount > 0
            ? Math.Round(ratingSum / ratingsCount, 1)
            : 0.0;

        // New incoming requests: ClientRequests assigned to lawyer and still Pending
        var newIncomingDirectRequestsCount = await _db.ClientRequests
            .CountAsync(cr => cr.LawyerServiceLawyerId == lawyerId
                           && cr.Status == ClientRequestStatus.Pending, cancellationToken);
                           
        var newIncomingOffersCount = await _db.RequestOffers
            .CountAsync(ro => ro.LawyerId == lawyerId
                           && ro.Status == OfferStatus.Pending, cancellationToken);
                           
        var newIncomingRequestsCount = newIncomingDirectRequestsCount + newIncomingOffersCount;

        // "Monthly" earnings -> currently calculates all-time Wallet Credits given there is no CreatedAt on WalletTransaction in DB schema.
        var monthlyEarnings = await _db.WalletTransactions
            .Where(wt => wt.Wallet.LawyerId == lawyerId && wt.Type == TransactionType.Credit)
            .Select(wt => wt.Amount)
            .SumAsync(cancellationToken);

        // Open Cases Count (Open or InProgress)
        var openCasesCount = await _db.Cases
            .CountAsync(c => c.LawyerId == lawyerId
                          && (c.Status == CaseStatus.Open || c.Status == CaseStatus.InProgress), cancellationToken);

        // Completed Cases Count (Closed)
        var completedCasesCount = await _db.Cases
            .CountAsync(c => c.LawyerId == lawyerId
                          && c.Status == CaseStatus.Closed, cancellationToken);

        var analyticsResult = new LawyerAnalyticsDto
        {
            AverageRating = ratingAverage,
            NewIncomingRequestsCount = newIncomingRequestsCount,
            MonthlyEarnings = monthlyEarnings,
            OpenCasesCount = openCasesCount,
            CompletedCasesCount = completedCasesCount
        };

        return Result<LawyerAnalyticsDto>.Success(analyticsResult);
    }
}
