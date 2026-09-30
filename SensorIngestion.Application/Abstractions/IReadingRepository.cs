using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Application.Abstractions;

public interface IReadingRepository
{
    Task<int> InsertNewAsync(List<SensorReading> readings, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);
}