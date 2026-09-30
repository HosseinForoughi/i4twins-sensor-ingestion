using SensorIngestion.Domain.Validation.Abstractions;
using SensorIngestion.Domain.Validation.Data;

namespace SensorIngestion.Domain.Validation.Implementations;

public class ReadingSemanticValidator : IReadingSemanticValidator
{
    private readonly List<IReadingSemanticRule> _rules;

    public ReadingSemanticValidator(List<IReadingSemanticRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        if (rules.Count == 0)
            throw new ArgumentException("At least one semantic rule is required.", nameof(rules));
        _rules = rules;
    }

    public ReadingValidationResult Validate(ReadingCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        var errors = new List<ReadingValidationError>();

        foreach (var rule in _rules)
        {
            var error = rule.Evaluate(candidate);
            if (error is not null)
                errors.Add(error);
        }

        return errors.Count == 0
            ? ReadingValidationResult.Success()
            : ReadingValidationResult.Failure(errors);
    }
}