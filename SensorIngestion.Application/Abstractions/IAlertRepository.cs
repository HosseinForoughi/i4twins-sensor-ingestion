using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Application.Abstractions;

public interface IAlertRepository
{
    Task<List<Alert>> ListAsync(CancellationToken cancellationToken = default);

    Task<int> InsertNewAsync(List<Alert> alerts, CancellationToken cancellationToken = default);
}