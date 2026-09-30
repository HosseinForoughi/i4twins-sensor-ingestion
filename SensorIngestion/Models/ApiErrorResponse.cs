namespace SensorIngestion.Api.Models;

/// <summary>
/// Error payload returned when a request fails.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// Human-readable description of what went wrong.
    /// </summary>
    /// <example>to must be greater than from.</example>
    public string Error { get; init; } = string.Empty;

    /// <summary>
    /// Correlation id for the request (also returned in the <c>X-Correlation-Id</c> header).
    /// </summary>
    /// <example>9f3c2a1b0e8d47c6a1b2c3d4e5f60718</example>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Distributed trace id for the request span.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Optional detailed error information (Development only for 5xx responses).
    /// </summary>
    public string? Detail { get; init; }
}