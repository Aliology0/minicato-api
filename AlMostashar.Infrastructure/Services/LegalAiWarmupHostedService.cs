using AlMostashar.Application.Common.Exceptions;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlMostashar.Infrastructure.Services;

public sealed class LegalAiWarmupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LegalAiOptions _options;
    private readonly ILogger<LegalAiWarmupHostedService> _logger;

    public LegalAiWarmupHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<LegalAiOptions> options,
        ILogger<LegalAiWarmupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.WarmupOnStartup)
        {
            return;
        }

        try
        {
            var delaySeconds = Math.Max(0, _options.WarmupDelaySeconds);
            if (delaySeconds > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
            }

            using var scope = _scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<ILegalAiClient>();
            await client.WarmupAsync(stoppingToken);

            _logger.LogInformation("Legal AI warmup completed.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (LegalAiServiceUnavailableException ex)
        {
            _logger.LogWarning(
                "Legal AI warmup failed. ExceptionType={ExceptionType}",
                ex.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Legal AI warmup failed unexpectedly. ExceptionType={ExceptionType}",
                ex.GetType().Name);
        }
    }
}
