using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Wallets.Queries.GetMyWallet;

public record GetMyWalletQuery : IRequest<Result<WalletSummaryDto>>;
