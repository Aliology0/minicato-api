using AlMostashar.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AlMostashar.Domain.Entities;
using System.Linq;

namespace AlMostashar.Infrastructure.Services;

public class DevelopmentStorageRepairHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DevelopmentStorageRepairHostedService> _logger;

    public DevelopmentStorageRepairHostedService(IServiceProvider services, IWebHostEnvironment env, ILogger<DevelopmentStorageRepairHostedService> logger)
    {
        _services = services;
        _env = env;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
            return;

        // Run once at startup to attempt to repair any persisted development storage URLs
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        // Development-only repair is no longer necessary when using the production LocalFileStorageService.
        var storage = scope.ServiceProvider.GetService<DevelopmentLocalStorageService>();
        if (storage == null)
        {
            _logger.LogDebug("DevelopmentLocalStorageService not registered; skipping development-only repair.");
            return;
        }

        try
        {
            // Repair Users.AvatarUrl and Lawyers.* document urls
            var users = await db.Users.ToListAsync(cancellationToken);
            var repaired = 0;
            foreach (var u in users)
            {
                if (string.IsNullOrWhiteSpace(u.AvatarUrl)) continue;
                if (!u.AvatarUrl.Contains("/api/development/storage/", StringComparison.OrdinalIgnoreCase)) continue;

                var token = u.AvatarUrl.Split('/').Last();
                if (storage.TryResolveDownload(token, out var path))
                {
                    var fileKey = Path.GetFileName(path);
                    var newUrl = storage.GetPublicUrl(fileKey);
                    u.AvatarUrl = newUrl;
                    repaired++;
                }
                else
                {
                    _logger.LogInformation("Could not resolve token for User {UserId}", u.Id);
                }
            }

            var lawyers = await db.Lawyers.ToListAsync(cancellationToken);
            foreach (var l in lawyers)
            {
                foreach (var prop in new[] { nameof(l.SSN_Url), nameof(l.SyndicateCardUrl), nameof(l.PracticeCertificatesUrl), nameof(l.AvatarUrl) })
                {
                    var val = (string?)typeof(Lawyer).GetProperty(prop)!.GetValue(l);
                    if (string.IsNullOrWhiteSpace(val)) continue;
                    if (!val.Contains("/api/development/storage/", StringComparison.OrdinalIgnoreCase)) continue;

                    var token = val.Split('/').Last();
                    if (storage.TryResolveDownload(token, out var path))
                    {
                        var fileKey = Path.GetFileName(path);
                        var newUrl = storage.GetPublicUrl(fileKey);
                        typeof(Lawyer).GetProperty(prop)!.SetValue(l, newUrl);
                        repaired++;
                    }
                    else
                    {
                        _logger.LogInformation("Could not resolve token for Lawyer {LawyerId} property {Prop}", l.Id, prop);
                    }
                }
            }

            if (repaired > 0)
            {
                await db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Repaired {Count} development storage URLs.", repaired);
            }
            else
            {
                _logger.LogInformation("No development storage URLs repaired.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during development storage URL repair.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
