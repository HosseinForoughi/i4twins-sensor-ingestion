using SensorIngestion.Domain.Deduplication.Models;
using SensorIngestion.Domain.ValueObjects;

namespace SensorIngestion.Domain.Deduplication.Abstractions;

public interface IReadingDeduplicator
{
    ReadingDeduplicationResult<T> Deduplicate<T>(List<T> items, Func<T, ReadingNaturalKey> keySelector);
}