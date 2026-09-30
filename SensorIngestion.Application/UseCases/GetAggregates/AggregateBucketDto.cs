namespace SensorIngestion.Application.UseCases.GetAggregates;

public class AggregateBucketDto
{
    public AggregateBucketDto(
        DateTimeOffset bucketStart,
        int count,
        double average,
        double min,
        double max)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        BucketStart = bucketStart;
        Count = count;
        Average = average;
        Min = min;
        Max = max;
    }

    public DateTimeOffset BucketStart { get; }

    public int Count { get; }

    public double Average { get; }

    public double Min { get; }

    public double Max { get; }
}
