using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Application.Abstractions;

public interface IReadingRepository
{
    Task<int> InsertNewAsync(List<SensorReading> readings, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<List<SensorReading>> ListUnprocessedBatchAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<List<string>> ListDeviceIdsForMetricAsync(string metric, CancellationToken cancellationToken = default);

    Task<List<SensorReading>> ListByDeviceAndMetricTrackedAsync(string deviceId, string metric, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    void ClearTracking();
}