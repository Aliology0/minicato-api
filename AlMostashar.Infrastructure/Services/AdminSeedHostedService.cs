using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Infrastructure.Services;

public sealed class AdminSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AdminSeedHostedService> _logger;

    public AdminSeedHostedService(IServiceScopeFactory scopeFactory, ILogger<AdminSeedHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<AdminSeedService>();
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed the initial admin account.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
