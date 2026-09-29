using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.SustainedAbove.Abstractions;
using SensorIngestion.Domain.SustainedAbove.Models;

namespace SensorIngestion.Domain.SustainedAbove.Implementations;

public class BatchSustainedAboveEvaluator : ISustainedAboveEvaluator
{
    public SustainedAboveEvaluationResult Evaluate(Rule rule, List<SensorReading> readings)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(readings);

        EnsureSustainedAboveRule(rule);

        var threshold = rule.Threshold!.Value;
        var minimumDuration = TimeSpan.FromSeconds(rule.DurationSeconds!.Value);

        var ordered = readings
            .Where(r => rule.AppliesTo(r.DeviceId, r.Metric))
            .OrderBy(r => r.Timestamp)
            .ThenBy(r => r.Sequence)
            .ToList();

        var qualifying = new List<SustainedEpisode>();

        var inEpisode = false;
        DateTimeOffset episodeStart = default;
        DateTimeOffset lastAboveTs = default;
        double peak = double.MinValue;
        string? deviceId = null;
        string? metric = null;

        foreach (var reading in ordered)
        {
            var above = reading.Value > threshold;

            if (above)
            {
                if (!inEpisode)
                {
                    inEpisode = true;
                    episodeStart = reading.Timestamp;
                    peak = reading.Value;
                    deviceId = reading.DeviceId;
                    metric = reading.Metric;
                }
                else if (reading.Value > peak)
                {
                    peak = reading.Value;
                }

                lastAboveTs = reading.Timestamp;
            }
            else if (inEpisode)
            {
                TryAddEpisode(
                    qualifying,
                    rule.RuleId,
                    deviceId!,
                    metric!,
                    episodeStart,
                    endTs: reading.Timestamp,
                    peak,
                    minimumDuration);

                inEpisode = false;
            }
        }

        if (inEpisode)
        {
            TryAddEpisode(
                qualifying,
                rule.RuleId,
                deviceId!,
                metric!,
                episodeStart,
                endTs: lastAboveTs,
                peak,
                minimumDuration);
        }

        return new SustainedAboveEvaluationResult(qualifying);
    }

    private static void EnsureSustainedAboveRule(Rule rule)
    {
        if (rule.Operator != RuleOperator.SustainedAbove)
        {
            throw new InvalidOperationException($"Rule '{rule.RuleId}' operator is {rule.Operator}; expected SustainedAbove.");
        }

        if (rule.Threshold is null || !double.IsFinite(rule.Threshold.Value))
        {
            throw new InvalidOperationException($"Rule '{rule.RuleId}' requires a finite Threshold for SustainedAbove.");
        }

        if (rule.DurationSeconds is null or <= 0)
        {
            throw new InvalidOperationException($"Rule '{rule.RuleId}' requires a positive DurationSeconds for SustainedAbove.");
        }
    }

    private static void TryAddEpisode(List<SustainedEpisode> target,
        string ruleId,
        string deviceId,
        string metric,
        DateTimeOffset startTs,
        DateTimeOffset endTs,
        double peakValue,
        TimeSpan minimumDuration)
    {
        var episode = new SustainedEpisode(ruleId, deviceId, metric, startTs, endTs, peakValue);

        if (episode.Duration >= minimumDuration)
            target.Add(episode);
    }
}