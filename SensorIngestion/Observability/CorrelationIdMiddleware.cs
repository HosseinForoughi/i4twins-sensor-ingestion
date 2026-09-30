using System.Diagnostics;
using SensorIngestion.Application.Observability;

namespace SensorIngestion.Api.Observability;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerFactory _loggerFactory;

    public CorrelationIdMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = CorrelationId.GetOrCreate(context);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationId.HeaderName] = correlationId;
            return Task.CompletedTask;
        });
        context.TraceIdentifier = correlationId;

        using var activity = ApplicationTelemetry.StartActivity(
            $"HTTP {context.Request.Method} {context.Request.Path}",
            ActivityKind.Server);

        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.route", context.Request.Path.Value);
        activity?.SetTag("correlation.id", correlationId);

        var logger = _loggerFactory.CreateLogger("SensorIngestion.Request");
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = correlationId,
                   ["TraceId"] = activity?.TraceId.ToString() ?? correlationId
               }))
        {
            await _next(context);
        }

        activity?.SetTag("http.status_code", context.Response.StatusCode);
    }
}