using AlMostashar.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AlMostashar.Infrastructure.Options;

namespace AlMostashar.Infrastructure.Services;

public sealed class UnlinkedDocumentCleanupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UnlinkedDocumentCleanupHostedService> _logger;
    private readonly DocumentCleanupOptions _options;
    public UnlinkedDocumentCleanupHostedService(IServiceScopeFactory scopeFactory, ILogger<UnlinkedDocumentCleanupHostedService> logger, IOptions<DocumentCleanupOptions> options) { _scopeFactory = scopeFactory; _logger = logger; _options = options.Value; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(Math.Max(1, _options.IntervalHours)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var cleanup = scope.ServiceProvider.GetRequiredService<IUnlinkedDocumentCleanupService>();
                await cleanup.CleanupAsync(DateTime.UtcNow.AddHours(-Math.Max(1, _options.RetentionHours)), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex) { _logger.LogError(ex, "Failed to clean up stale unlinked documents."); }
        }
    }
}
