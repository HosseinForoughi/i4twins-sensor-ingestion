using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using SensorIngestion.Api.Models;

namespace SensorIngestion.Api.Observability;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
            return false;

        var correlationId = CorrelationId.TryGet(httpContext) ?? httpContext.TraceIdentifier;
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        var (statusCode, publicMessage) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. CorrelationId={CorrelationId}, TraceId={TraceId}, Path={Path}",
                correlationId,
                traceId,
                httpContext.Request.Path.Value);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request rejected. CorrelationId={CorrelationId}, TraceId={TraceId}, StatusCode={StatusCode}, Path={Path}",
                correlationId,
                traceId,
                statusCode,
                httpContext.Request.Path.Value);
        }

        if (httpContext.Response.HasStarted)
            return false;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.Headers[CorrelationId.HeaderName] = correlationId;

        var body = new ApiErrorResponse
        {
            Error = publicMessage,
            CorrelationId = correlationId,
            TraceId = traceId,
            Detail = _environment.IsDevelopment() && statusCode >= StatusCodes.Status500InternalServerError
                ? exception.ToString()
                : null
        };

        await httpContext.Response.WriteAsJsonAsync(body, cancellationToken);
        return true;
    }

    private static (int StatusCode, string Message) MapException(Exception exception) =>
        exception switch
        {
            ArgumentException argumentException => (StatusCodes.Status400BadRequest, argumentException.Message),
            FileNotFoundException fileNotFoundException => (
                StatusCodes.Status500InternalServerError,
                fileNotFoundException.Message),
            InvalidOperationException invalidOperationException => (
                StatusCodes.Status500InternalServerError,
                invalidOperationException.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };
}