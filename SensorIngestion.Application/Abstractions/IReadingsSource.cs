using SensorIngestion.Application.Readings;

namespace SensorIngestion.Application.Abstractions;

public interface IReadingsSource
{
    Task<ReadingsLoadResult> LoadAsync(CancellationToken cancellationToken = default);
}