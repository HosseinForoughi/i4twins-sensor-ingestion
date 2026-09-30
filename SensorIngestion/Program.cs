using SensorIngestion.Api.Swagger;
using SensorIngestion.Application;
using SensorIngestion.Application.UseCases.SeedRules;
using SensorIngestion.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSensorIngestionSwagger();

var app = builder.Build();

await app.Services.MigrateAsync();

using (var scope = app.Services.CreateScope())
{
    var seedRules = scope.ServiceProvider.GetRequiredService<SeedRulesUseCase>();
    await seedRules.ExecuteAsync();
}

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
