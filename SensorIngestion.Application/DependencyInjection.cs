using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorIngestion.Application.Options;
using SensorIngestion.Application.UseCases.IngestReadings;
using SensorIngestion.Application.UseCases.SeedRules;
using SensorIngestion.Domain.Deduplication.Abstractions;
using SensorIngestion.Domain.Deduplication.Implementations;
using SensorIngestion.Domain.Validation.Abstractions;
using SensorIngestion.Domain.Validation.Implementations;
using SensorIngestion.Domain.Validation.Implementations.Rules;

namespace SensorIngestion.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<RulesOptions>(configuration.GetSection(RulesOptions.SectionName));
        services.Configure<ReadingsOptions>(configuration.GetSection(ReadingsOptions.SectionName));

        AddDomainServices(services);

        services.AddScoped<SeedRulesUseCase>();
        services.AddScoped<IngestReadingsUseCase>();

        return services;
    }

    private static void AddDomainServices(IServiceCollection services)
    {
        services.AddSingleton<IReadingSemanticRule, DeviceIdRequiredRule>();
        services.AddSingleton<IReadingSemanticRule, MetricRequiredRule>();
        services.AddSingleton<IReadingSemanticRule, SequenceRequiredRule>();
        services.AddSingleton<IReadingSemanticRule, ValueFiniteRule>();
        services.AddSingleton<IReadingSemanticRule, TimestampValidRule>();
        services.AddSingleton<IReadingSemanticValidator, ReadingSemanticValidator>();

        services.AddSingleton<IReadingDeduplicator, FirstWinsReadingDeduplicator>();
    }
}