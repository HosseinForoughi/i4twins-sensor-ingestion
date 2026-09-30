using System.Diagnostics;

namespace SensorIngestion.Api.Observability;

public static class CorrelationId
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    public static string GetOrCreate(HttpContext context)
    {
        if (context.Items.TryGetValue(ItemKey, out var existing) && existing is string value && !string.IsNullOrWhiteSpace(value))
            return value;

        if (context.Request.Headers.TryGetValue(HeaderName, out var header) &&
            !string.IsNullOrWhiteSpace(header))
        {
            value = header.ToString();
        }
        else
        {
            value = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
        }

        context.Items[ItemKey] = value;
        return value;
    }

    public static string? TryGet(HttpContext context) =>
        context.Items.TryGetValue(ItemKey, out var value) ? value as string : null;
}