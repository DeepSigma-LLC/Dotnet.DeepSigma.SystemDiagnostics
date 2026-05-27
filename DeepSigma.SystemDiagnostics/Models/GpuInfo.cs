namespace DeepSigma.SystemDiagnostics.Models;

public sealed record GpuInfo(
    string Name,
    string? Vendor,
    string? DriverVersion,
    ulong? AdapterRamBytes);
