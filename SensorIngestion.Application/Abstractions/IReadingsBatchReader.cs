using SensorIngestion.Application.Readings;

namespace SensorIngestion.Application.Abstractions;

public interface IReadingsBatchReader : IAsyncDisposable
{
    Task<ReadingsBatch?> ReadNextAsync(CancellationToken cancellationToken = default);
}
