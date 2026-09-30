using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SensorIngestion.Infrastructure.Persistence;
using SensorIngestion.Tests.Integration.Fixtures;

namespace SensorIngestion.Tests.EndToEnd.Fixtures;

public class SensorIngestionWebApplicationFactory : IAsyncLifetime
{
    private readonly PostgreSqlFixture _postgres = new();
    private HostFactory? _host;

    public HttpClient CreateClient() => EnsureHost().CreateClient();

    public IServiceProvider Services => EnsureHost().Services;

    public async Task InitializeAsync()
    {
        await _postgres.InitializeAsync();
        _host = new HostFactory(_postgres.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (_host is not null)
            await _host.DisposeAsync();

        await _postgres.DisposeAsync();
    }

    private HostFactory EnsureHost() =>
        _host ?? throw new InvalidOperationException("Web application factory was not initialized.");

    private class HostFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public HostFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            var dataDirectory = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SensorIngestion", "Data"));

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SensorIngestion"] = _connectionString,
                    ["Readings:FilePath"] = Path.Combine(dataDirectory, "readings.jsonl"),
                    ["Rules:SeedFilePath"] = Path.Combine(dataDirectory, "rules.json")
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<SensorIngestionDbContext>));
                services.RemoveAll(typeof(SensorIngestionDbContext));

                services.AddDbContext<SensorIngestionDbContext>(options =>
                    options.UseNpgsql(_connectionString));
            });
        }
    }
}

[CollectionDefinition(Name)]
public class EndToEndCollection : ICollectionFixture<SensorIngestionWebApplicationFactory>
{
    public const string Name = "EndToEnd";
}