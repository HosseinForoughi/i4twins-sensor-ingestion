using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorIngestion.Infrastructure.Persistence;

namespace SensorIngestion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(  this IServiceCollection services,       IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        AddPersistence(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SensorIngestion");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'SensorIngestion' is missing. " +
                "Add it under ConnectionStrings:SensorIngestion in appsettings.");
        }

        services.AddDbContext<SensorIngestionDbContext>(options =>  options.UseSqlite(connectionString));
    }
}