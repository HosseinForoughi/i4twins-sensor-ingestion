using Microsoft.AspNetCore.Mvc.Testing;

namespace SensorIngestion.Tests.Smoke;

public class ApiHostFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();

    public HttpClient CreateClient() => _factory.CreateClient();

    public IServiceProvider Services => _factory.Services;

    public void Dispose() => _factory.Dispose();
}