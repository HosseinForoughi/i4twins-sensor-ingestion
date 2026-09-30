using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Application.Abstractions;

public interface IAlertRepository
{
    Task<List<Alert>> ListForStreamAsync(string ruleId, string deviceId, string metric, CancellationToken cancellationToken = default);

    Task<int> InsertNewAsync(List<Alert> alerts, CancellationToken cancellationToken = default);
}