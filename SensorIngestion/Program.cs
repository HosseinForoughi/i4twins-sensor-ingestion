using SensorIngestion.Application;
using SensorIngestion.Application.UseCases.SeedRules;
using SensorIngestion.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.Services.MigrateAsync();

using (var scope = app.Services.CreateScope())
{
    var seedRules = scope.ServiceProvider.GetRequiredService<SeedRulesUseCase>();
    await seedRules.ExecuteAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
