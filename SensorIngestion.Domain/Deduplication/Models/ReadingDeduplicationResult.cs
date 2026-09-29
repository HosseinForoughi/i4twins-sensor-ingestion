namespace SensorIngestion.Domain.Deduplication.Models;

public class ReadingDeduplicationResult<T>
{
    public ReadingDeduplicationResult(List<T> unique, List<T> duplicates)
    {
        ArgumentNullException.ThrowIfNull(unique);
        ArgumentNullException.ThrowIfNull(duplicates);

        Unique = unique;
        Duplicates = duplicates;
    }

    public List<T> Unique { get; }

    public List<T> Duplicates { get; }

    public int UniqueCount => Unique.Count;

    public int DuplicatesRemovedCount => Duplicates.Count;
}