using Microsoft.EntityFrameworkCore;
using SensorIngestion.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SensorIngestion.Tests.Integration.Fixtures;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("sensor_ingestion_tests")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public SensorIngestionDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SensorIngestionDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var dbContext = new SensorIngestionDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    public async Task ResetDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}

[CollectionDefinition(Name)]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSql";
}