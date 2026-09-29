namespace SensorIngestion.Domain.Validation.Data;

public class ReadingValidationResult
{
    private ReadingValidationResult(bool isValid, List<ReadingValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public bool IsValid { get; }

    public List<ReadingValidationError> Errors { get; }

    public static ReadingValidationResult Success()
    {
        return new(true, []);
    }

    public static ReadingValidationResult Failure(List<ReadingValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        var list = errors.ToList();
        if (list.Count == 0)
            throw new ArgumentException("At least one error is required for failure.", nameof(errors));

        return new(false, list);
    }
}
