namespace SensorIngestion.Api.Models;

/// <summary>
/// Error payload returned when a request fails validation.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// Human-readable description of what went wrong.
    /// </summary>
    /// <example>to must be greater than from.</example>
    public string Error { get; init; } = string.Empty;
}