using System.Diagnostics;

namespace SensorIngestion.Application.Observability;

/// <summary>
/// Shared <see cref="ActivitySource"/> for application-level spans.
/// </summary>
public static class ApplicationTelemetry
{
    public const string ServiceName = "SensorIngestion";

    public static readonly ActivitySource ActivitySource = new(ServiceName);

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal) =>
        ActivitySource.StartActivity(name, kind);
}
