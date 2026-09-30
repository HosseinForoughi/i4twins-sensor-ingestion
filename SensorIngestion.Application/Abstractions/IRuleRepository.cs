using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Application.Abstractions;

public interface IRuleRepository
{
    Task UpsertRangeAsync(List<Rule> rules, CancellationToken cancellationToken = default);

    Task<List<Rule>> ListAsync(CancellationToken cancellationToken = default);
}