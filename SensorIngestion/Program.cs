using SensorIngestion.Api.Observability;
using SensorIngestion.Api.Swagger;
using SensorIngestion.Application;
using SensorIngestion.Application.UseCases.SeedRules;
using SensorIngestion.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSensorIngestionObservability();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSensorIngestionSwagger();

var app = builder.Build();
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("SensorIngestion.Startup");

try
{
    startupLogger.LogInformation("Applying database migrations.");
    await app.Services.MigrateAsync();

    using var scope = app.Services.CreateScope();
    var seedRules = scope.ServiceProvider.GetRequiredService<SeedRulesUseCase>();
    startupLogger.LogInformation("Seeding rules from configured seed file.");
    await seedRules.ExecuteAsync();
    startupLogger.LogInformation("Startup initialization completed.");
}
catch (Exception ex)
{
    startupLogger.LogCritical(ex, "Startup initialization failed.");
    throw;
}

app.UseSensorIngestionObservability();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sensor Ingestion API v1");
    options.DocumentTitle = "Sensor Ingestion API";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
