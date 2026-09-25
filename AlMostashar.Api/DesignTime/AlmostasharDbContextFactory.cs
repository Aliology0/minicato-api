using AlMostashar.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlMostashar.Api.DesignTime;

public sealed class AlmostasharDbContextFactory : IDesignTimeDbContextFactory<AlmostasharDbContext>
{
    public AlmostasharDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("AlmostasharSqlServer")
            ?? "Server=(localdb)\\mssqllocaldb;Database=AlMostashar_DesignTime;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<AlmostasharDbContext>()
            .UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null))
            .Options;

        return new AlmostasharDbContext(options, new NoOpPublisher());
    }

    private sealed class NoOpPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
