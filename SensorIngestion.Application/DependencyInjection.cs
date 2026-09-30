using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorIngestion.Application.Options;
using SensorIngestion.Application.UseCases.SeedRules;

namespace SensorIngestion.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<RulesOptions>(configuration.GetSection(RulesOptions.SectionName));
        services.AddScoped<SeedRulesUseCase>();

        return services;
    }
}