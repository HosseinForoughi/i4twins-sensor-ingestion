using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.SustainedAbove.Abstractions;
using SensorIngestion.Domain.SustainedAbove.Models;

namespace SensorIngestion.Domain.SustainedAbove.Implementations;

public class SustainedAboveViolationApplier : ISustainedAboveViolationApplier
{
    public int Apply(Rule rule, List<SustainedEpisode> episodes, List<SensorReading> readings)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(episodes);
        ArgumentNullException.ThrowIfNull(readings);

        if (rule.Operator != RuleOperator.SustainedAbove) throw new InvalidOperationException($"Rule '{rule.RuleId}' is not SustainedAbove.");

        var threshold = rule.Threshold!.Value;
        var duration = TimeSpan.FromSeconds(rule.DurationSeconds!.Value);

        var applicableReadings = readings.Where(r => rule.AppliesTo(r.DeviceId, r.Metric)).ToList();

        var added = 0;

        foreach (var episode in episodes)
        {
            var violationStart = episode.StartTs.Add(duration);

            foreach (var reading in applicableReadings)
            {
                if (reading.Value <= threshold)
                    continue;

                if (reading.Timestamp < episode.StartTs || reading.Timestamp > episode.EndTs)
                    continue;

                if (reading.Timestamp < violationStart)
                    continue;

                var before = reading.Violations.Count;
                reading.ApplyRuleViolation(new ReadingViolation(
                        rule.RuleId,
                        $"Value {reading.Value} stayed above threshold {threshold} for at least {rule.DurationSeconds}s of event time (rule '{rule.RuleId}')."));

                if (reading.Violations.Count > before)
                    added++;
            }
        }

        return added;
    }
}