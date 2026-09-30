using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Infrastructure.Persistence;
using SensorIngestion.Infrastructure.Persistence.Repositories;
using SensorIngestion.Infrastructure.Readings;
using SensorIngestion.Infrastructure.Rules;

namespace SensorIngestion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        AddPersistence(services, configuration);
        AddRulesSeed(services);
        AddReadingsSource(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SensorIngestion");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'SensorIngestion' is missing. Add it under ConnectionStrings:SensorIngestion in appsettings.");

        services.AddDbContext<SensorIngestionDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IRuleRepository, RuleRepository>();
        services.AddScoped<IReadingRepository, ReadingRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
    }

    private static void AddRulesSeed(IServiceCollection services)
    {
        services.AddScoped<IRulesSeedSource, JsonRulesSeedSource>();
    }

    private static void AddReadingsSource(IServiceCollection services)
    {
        services.AddScoped<IReadingsSource, JsonlReadingsSource>();
    }
}