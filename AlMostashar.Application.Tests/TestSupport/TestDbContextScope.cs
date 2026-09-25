using AlMostashar.Infrastructure.Data;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Tests.TestSupport;

public sealed class TestDbContextScope : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly IPublisher _publisher;
    private readonly DbContextOptions<AlmostasharDbContext> _options;

    public AlmostasharDbContext DbContext { get; private set; }

    private TestDbContextScope(
        SqliteConnection connection,
        IPublisher publisher,
        DbContextOptions<AlmostasharDbContext> options,
        AlmostasharDbContext dbContext)
    {
        _connection = connection;
        _publisher = publisher;
        _options = options;
        DbContext = dbContext;
    }

    public static async Task<TestDbContextScope> CreateAsync(IPublisher? publisher = null)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AlmostasharDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        var usedPublisher = publisher ?? new NoOpPublisher();
        var dbContext = new AlmostasharDbContext(options, usedPublisher);
        await dbContext.Database.EnsureCreatedAsync();

        return new TestDbContextScope(connection, usedPublisher, options, dbContext);
    }

    public AlmostasharDbContext CreateNewContext()
    {
        return new AlmostasharDbContext(_options, _publisher);
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

