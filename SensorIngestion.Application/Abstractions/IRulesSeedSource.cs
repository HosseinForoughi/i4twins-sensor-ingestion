using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Application.Abstractions;

public interface IRulesSeedSource
{
    Task<List<Rule>> LoadAsync(CancellationToken cancellationToken = default);
}