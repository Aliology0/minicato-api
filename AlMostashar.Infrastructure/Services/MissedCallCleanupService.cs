using AlMostashar.Domain.ValueObject.Enum;
using AlMostashar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Infrastructure.Services;

/// <summary>
/// Periodically marks unanswered calls as Missed.
/// Any CallSession still in the Initiated state for longer than 1 minute
/// is considered missed because the receiver did not pick up.
/// </summary>
public sealed class MissedCallCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MissedCallCleanupService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public MissedCallCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<MissedCallCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AlmostasharDbContext>();

                var cutoff = DateTime.UtcNow.AddMinutes(-1);
                var now = DateTime.UtcNow;

                var affected = await db.CallSessions
                    .Where(cs => cs.Status == CallStatus.Initiated && cs.StartTime <= cutoff)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(cs => cs.Status, CallStatus.Missed)
                        .SetProperty(cs => cs.EndTime, now)
                        .SetProperty(cs => cs.DurationInSeconds, cs => EF.Functions.DateDiffSecond(cs.StartTime, now)),
                        stoppingToken);

                if (affected > 0)
                {
                    _logger.LogInformation("Marked {Count} stale call(s) as Missed.", affected);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Application is shutting down — exit gracefully
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning up missed calls.");
            }
        }
    }
}
