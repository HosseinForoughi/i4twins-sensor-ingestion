using Microsoft.OpenApi.Models;
using SensorIngestion.Application.UseCases.ProcessPipeline;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace SensorIngestion.Api.Swagger;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSensorIngestionSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sensor Ingestion API",
                Version = "v1",
                Description =
                    "Ingests sensor readings from JSONL, evaluates instantaneous and SustainedAbove rules, " +
                    "raises cooldown-aware alerts, and returns time-bucket aggregates of acceptable data.",
                Contact = new OpenApiContact
                {
                    Name = "Sensor Ingestion Service"
                }
            });

            options.SupportNonNullableReferenceTypes();
            options.DescribeAllParametersInCamelCase();

            IncludeXmlComments(options, Assembly.GetExecutingAssembly(), includeControllerXmlComments: true);
            IncludeXmlComments(options, typeof(ProcessPipelineResult).Assembly, includeControllerXmlComments: false);
        });

        return services;
    }

    private static void IncludeXmlComments(
        SwaggerGenOptions options,
        Assembly assembly,
        bool includeControllerXmlComments)
    {
        var directory = Path.GetDirectoryName(assembly.Location);
        if (directory is null)
            return;

        var xmlPath = Path.Combine(directory, $"{assembly.GetName().Name}.xml");
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments);
    }
}