namespace SensorIngestion.Domain.Validation.Data;

public class ReadingValidationError
{
    public ReadingValidationError(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }
}