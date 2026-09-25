using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Wallets.Queries.GetAdminWithdrawalById;

public class GetAdminWithdrawalByIdQueryHandler : IRequestHandler<GetAdminWithdrawalByIdQuery, Result<WithdrawalRequestDto>>
{
    private readonly IAppDbContext _db;
    private readonly ISensitiveDataProtector _sensitiveDataProtector;

    public GetAdminWithdrawalByIdQueryHandler(
        IAppDbContext db,
        ISensitiveDataProtector sensitiveDataProtector)
    {
        _db = db;
        _sensitiveDataProtector = sensitiveDataProtector;
    }

    public async Task<Result<WithdrawalRequestDto>> Handle(GetAdminWithdrawalByIdQuery request, CancellationToken cancellationToken)
    {
        var withdrawal = await _db.WithdrawalRequests
            .AsNoTracking()
            .Include(wr => wr.Lawyer)
            .Where(wr => wr.Id == request.WithdrawalId)
            .FirstOrDefaultAsync(cancellationToken);

        if (withdrawal is null)
        {
            return Result<WithdrawalRequestDto>.Failure(new Error("Withdrawal.NotFound", "Withdrawal request was not found."));
        }

        var fullAccountDetails = _sensitiveDataProtector.Unprotect(withdrawal.AccountDetailsEncrypted);

        return Result<WithdrawalRequestDto>.Success(
            WithdrawalRequestDtoMapper.ToDto(
                withdrawal,
                accountDetailsFull: fullAccountDetails,
                lawyerName: withdrawal.Lawyer.FullName,
                lawyerEmail: withdrawal.Lawyer.Email));
    }
}
