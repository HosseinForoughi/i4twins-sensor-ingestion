using SensorIngestion.Api.Models;
using SensorIngestion.Application.UseCases.GetAggregates;
using SensorIngestion.Application.UseCases.ProcessPipeline;
using SensorIngestion.Tests.EndToEnd.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace SensorIngestion.Tests.EndToEnd.Api;

[Collection(EndToEndCollection.Name)]
public class IngestAndAggregatesApiTests
{
    private readonly HttpClient _client;

    public IngestAndAggregatesApiTests(SensorIngestionWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostIngest_WhenCalled_ReturnsOkWithPipelineResult()
    {
        // Arrange
        // HttpClient is created by the WebApplicationFactory fixture.

        // Act
        var response = await _client.PostAsync("/api/ingest", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ProcessPipelineResult>();
        Assert.NotNull(body);
        Assert.True(body!.Ingest.TotalLinesRead > 0);
    }

    [Fact]
    public async Task GetAggregates_AfterIngest_ReturnsBucketsForAcceptableData()
    {
        // Arrange
        var ingestResponse = await _client.PostAsync("/api/ingest", content: null);
        ingestResponse.EnsureSuccessStatusCode();

        var url =
            "/api/aggregates?deviceId=PUMP-01&metric=temperature" +
            "&from=2025-06-01T08:00:00Z&to=2025-06-01T09:00:00Z&bucketSeconds=60";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var buckets = await response.Content.ReadFromJsonAsync<List<AggregateBucketDto>>();
        Assert.NotNull(buckets);
        Assert.NotEmpty(buckets!);
        Assert.All(buckets!, b => Assert.True(b.Count > 0));
    }

    [Fact]
    public async Task PostIngest_WhenCalledTwice_IsIdempotentForStoredReadings()
    {
        // Arrange
        var first = await _client.PostAsync("/api/ingest", content: null);
        first.EnsureSuccessStatusCode();
        var firstBody = await first.Content.ReadFromJsonAsync<ProcessPipelineResult>();

        // Act
        var second = await _client.PostAsync("/api/ingest", content: null);
        second.EnsureSuccessStatusCode();
        var secondBody = await second.Content.ReadFromJsonAsync<ProcessPipelineResult>();

        // Assert
        Assert.NotNull(firstBody);
        Assert.NotNull(secondBody);
        Assert.Equal(0, secondBody!.Ingest.NewlyInsertedReadings);
        Assert.Equal(0, secondBody.SustainedAbove.AlertsInserted);
    }

    [Fact]
    public async Task GetAggregates_WhenRangeInvalid_ReturnsBadRequestWithCorrelation()
    {
        // Arrange
        const string correlationId = "test-correlation-123";
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/aggregates?deviceId=PUMP-01&metric=temperature" +
            "&from=2025-06-01T09:00:00Z&to=2025-06-01T08:00:00Z&bucketSeconds=60");
        request.Headers.Add("X-Correlation-Id", correlationId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Correlation-Id", out var headerValues));
        Assert.Equal(correlationId, headerValues.Single());

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Error));
        Assert.Equal(correlationId, body.CorrelationId);
        Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
    }
}