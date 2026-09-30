namespace SensorIngestion.Application.UseCases.GetAggregates;

public class GetAggregatesRequest
{
    public GetAggregatesRequest(
        string deviceId,
        string metric,
        DateTimeOffset from,
        DateTimeOffset to,
        int bucketSeconds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);

        if (to <= from)
            throw new ArgumentException("'to' must be greater than 'from'.", nameof(to));

        if (bucketSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(bucketSeconds), "bucketSeconds must be positive.");

        DeviceId = deviceId;
        Metric = metric;
        From = from.ToUniversalTime();
        To = to.ToUniversalTime();
        BucketSeconds = bucketSeconds;
    }

    public string DeviceId { get; }

    public string Metric { get; }

    public DateTimeOffset From { get; }

    public DateTimeOffset To { get; }

    public int BucketSeconds { get; }
}
