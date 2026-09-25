using AlMostashar.Application.Common.Interfaces;
using MediatR;

namespace AlMostashar.Application.Common.Behaviors
{
    /// <summary>
    /// Pipeline behavior that wraps command handlers in a database transaction.
    /// Only applies to commands marked with <see cref="ITransactionalCommand"/>.
    /// Uses <see cref="IAppDbContext.ExecuteInTransactionAsync"/> which handles
    /// EF Core's execution strategy (retry on failure) internally.
    /// </summary>
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ITransactionalCommand
    {
        private readonly IAppDbContext _dbContext;

        public TransactionBehavior(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            TResponse response = default!;

            await _dbContext.ExecuteInTransactionAsync(async (ct) =>
            {
                response = await next();
            }, cancellationToken);

            return response;
        }
    }
}
