namespace SensorIngestion.Application.UseCases.GetAggregates;

/// <summary>
/// One non-empty time bucket of acceptable readings for a device and metric.
/// </summary>
public class AggregateBucketDto
{
    public AggregateBucketDto(DateTimeOffset bucketStart,
        int count,
        double average,
        double min,
        double max)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));

        BucketStart = bucketStart;
        Count = count;
        Average = average;
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Start timestamp of the bucket interval (UTC).
    /// </summary>
    /// <example>2025-06-01T08:00:00Z</example>
    public DateTimeOffset BucketStart { get; }

    /// <summary>
    /// Number of acceptable readings in the bucket.
    /// </summary>
    /// <example>12</example>
    public int Count { get; }

    /// <summary>
    /// Average reading value in the bucket.
    /// </summary>
    /// <example>67.5</example>
    public double Average { get; }

    /// <summary>
    /// Minimum reading value in the bucket.
    /// </summary>
    /// <example>61.2</example>
    public double Min { get; }

    /// <summary>
    /// Maximum reading value in the bucket.
    /// </summary>
    /// <example>73.8</example>
    public double Max { get; }
}