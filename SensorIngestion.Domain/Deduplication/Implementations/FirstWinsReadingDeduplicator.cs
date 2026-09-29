using SensorIngestion.Domain.Deduplication.Abstractions;
using SensorIngestion.Domain.Deduplication.Models;
using SensorIngestion.Domain.ValueObjects;

namespace SensorIngestion.Domain.Deduplication.Implementations;

public class FirstWinsReadingDeduplicator : IReadingDeduplicator
{
    public ReadingDeduplicationResult<T> Deduplicate<T>(List<T> items, Func<T, ReadingNaturalKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(keySelector);

        var unique = new List<T>();
        var duplicates = new List<T>();
        var seen = new HashSet<ReadingNaturalKey>();

        foreach (var item in items)
        {
            var key = keySelector(item);

            if (seen.Add(key))
                unique.Add(item);
            else
                duplicates.Add(item);
        }

        return new ReadingDeduplicationResult<T>(unique, duplicates);
    }
}