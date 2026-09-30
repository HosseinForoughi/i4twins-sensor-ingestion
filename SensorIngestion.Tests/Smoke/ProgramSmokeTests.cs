using Microsoft.Extensions.DependencyInjection;
using SensorIngestion.Application.UseCases.GetAggregates;
using SensorIngestion.Application.UseCases.IngestReadings;
using SensorIngestion.Application.UseCases.ProcessPipeline;
using SensorIngestion.Application.UseCases.SeedRules;

namespace SensorIngestion.Tests.Smoke;

public class ProgramSmokeTests : IClassFixture<ApiHostFixture>
{
    private readonly ApiHostFixture _factory;

    public ProgramSmokeTests(ApiHostFixture factory)
    {
        _factory = factory;
    }

    [Fact]
    public void CreateClient_WhenApplicationStarts_DoesNotThrow()
    {
        // Arrange
        // Act
        var exception = Record.Exception(() =>
        {
            using var client = _factory.CreateClient();
        });

        // Assert
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(typeof(SeedRulesUseCase))]
    [InlineData(typeof(IngestReadingsUseCase))]
    [InlineData(typeof(ProcessPipelineUseCase))]
    [InlineData(typeof(GetAggregatesUseCase))]
    public void ServiceProvider_WhenResolvingApplicationServices_ReturnsInstance(Type serviceType)
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act
        var service = scope.ServiceProvider.GetService(serviceType);

        // Assert
        Assert.NotNull(service);
    }
}