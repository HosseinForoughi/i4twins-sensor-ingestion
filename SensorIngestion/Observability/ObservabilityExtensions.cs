using System.Diagnostics;

namespace SensorIngestion.Api.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddSensorIngestionObservability(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    public static IApplicationBuilder UseSensorIngestionObservability(this IApplicationBuilder app)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseExceptionHandler();
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}