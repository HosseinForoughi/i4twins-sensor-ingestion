using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Application.Abstractions;

public interface IReadingRepository
{
    Task<int> InsertNewAsync(List<SensorReading> readings, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<List<SensorReading>> ListUnprocessedAsync(CancellationToken cancellationToken = default);

    Task<List<SensorReading>> ListAllTrackedAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}