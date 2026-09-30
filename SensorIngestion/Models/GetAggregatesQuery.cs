using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SensorIngestion.Api.Models;

/// <summary>
/// Query parameters for time-bucket aggregates of acceptable sensor readings.
/// </summary>
public class GetAggregatesQuery
{
    /// <summary>
    /// Device identifier to aggregate.
    /// </summary>
    /// <example>PUMP-01</example>
    [Required]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Metric name to aggregate (for example temperature, pressure, vibration).
    /// </summary>
    /// <example>temperature</example>
    [Required]
    public string Metric { get; set; } = string.Empty;

    /// <summary>
    /// Inclusive start of the aggregation window (UTC recommended).
    /// </summary>
    /// <example>2025-06-01T08:00:00Z</example>
    [Required]
    public DateTimeOffset From { get; set; }

    /// <summary>
    /// Exclusive end of the aggregation window (UTC recommended). Must be greater than <see cref="From"/>.
    /// </summary>
    /// <example>2025-06-01T09:00:00Z</example>
    [Required]
    public DateTimeOffset To { get; set; }

    /// <summary>
    /// Bucket width in seconds. Must be a positive integer.
    /// </summary>
    /// <example>60</example>
    [Required]
    [Range(1, int.MaxValue)]
    [DefaultValue(60)]
    public int BucketSeconds { get; set; } = 60;
}