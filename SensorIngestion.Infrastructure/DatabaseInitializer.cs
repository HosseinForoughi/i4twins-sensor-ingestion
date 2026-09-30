using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SensorIngestion.Infrastructure.Persistence;

namespace SensorIngestion.Infrastructure;

public static class DatabaseInitializer
{

    public static async Task MigrateAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SensorIngestionDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}