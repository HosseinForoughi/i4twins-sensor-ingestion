namespace SensorIngestion.Domain.Enums;

public enum ReadingClassification:byte
{
    Unprocessed = 1,
    Acceptable = 2,
    Unacceptable = 4
}
