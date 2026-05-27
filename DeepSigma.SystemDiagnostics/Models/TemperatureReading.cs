namespace DeepSigma.SystemDiagnostics.Models;

/// <summary>
/// A single temperature reading from one of the host's hardware sensors.
/// </summary>
/// <remarks>
/// Linux-only. On Windows <c>SystemDiagnostics.GetTemperatures()</c> always returns an empty
/// list — see the README for rationale.
/// </remarks>
/// <param name="ChipName">Sensor-chip name as exposed by <c>/sys/class/hwmon/hwmonX/name</c>
/// (e.g. <c>"coretemp"</c>, <c>"nvme"</c>, <c>"k10temp"</c>, <c>"acpitz"</c>).</param>
/// <param name="Label">Per-sensor label when the kernel provides one (e.g. <c>"Package id 0"</c>,
/// <c>"Core 0"</c>, <c>"Composite"</c>), otherwise <c>null</c>.</param>
/// <param name="Celsius">Current temperature in degrees Celsius.</param>
/// <param name="MaxCelsius">Warning threshold in degrees Celsius from <c>tempN_max</c>, or <c>null</c> if not provided.</param>
/// <param name="CritCelsius">Critical threshold in degrees Celsius from <c>tempN_crit</c>, or <c>null</c> if not provided.</param>
public sealed record TemperatureReading(
    string ChipName,
    string? Label,
    double Celsius,
    double? MaxCelsius,
    double? CritCelsius);
