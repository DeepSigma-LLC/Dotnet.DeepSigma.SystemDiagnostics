namespace DeepSigma.SystemDiagnostics.Models;

public sealed record TemperatureReading(
    string ChipName,
    string? Label,
    double Celsius,
    double? MaxCelsius,
    double? CritCelsius);
