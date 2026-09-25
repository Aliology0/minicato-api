using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Common.Helpers
{
    public static class EscrowQueryExtensions
    {
        /// <summary>
        /// A provider-aware helper that safely sums escrow amounts.
        /// Uses efficient database-side aggregation normally, but falls back to in-memory
        /// aggregation ONLY for the SQLite test provider due to its limitations with decimal aggregations.
        /// </summary>
        public static async Task<decimal> SumAmountSafeAsync(
            this IQueryable<Escrow> query, 
            IAppDbContext dbContext, 
            CancellationToken cancellationToken = default)
        {
            if (dbContext is DbContext efContext && efContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                // Fallback for SQLite tests: pull values and sum in memory
                var amounts = await query.Select(e => e.Amount).ToListAsync(cancellationToken);
                return amounts.Sum();
            }

            // Production path: Database-side aggregation
            return await query.SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;
        }
    }
}
