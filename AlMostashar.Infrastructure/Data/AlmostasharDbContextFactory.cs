using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlMostashar.Infrastructure.Data;

public sealed class AlmostasharDbContextFactory : IDesignTimeDbContextFactory<AlmostasharDbContext>
{
    public AlmostasharDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AlmostasharDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AlMostasharDesignTime;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new AlmostasharDbContext(options, new DesignTimePublisher());
    }

    private sealed class DesignTimePublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;
    }
}
