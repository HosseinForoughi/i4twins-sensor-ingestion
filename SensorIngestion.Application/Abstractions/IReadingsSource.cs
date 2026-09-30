namespace SensorIngestion.Application.Abstractions;

public interface IReadingsSource
{
    Task<IReadingsBatchReader> OpenBatchesAsync(int batchSize, CancellationToken cancellationToken = default);
}