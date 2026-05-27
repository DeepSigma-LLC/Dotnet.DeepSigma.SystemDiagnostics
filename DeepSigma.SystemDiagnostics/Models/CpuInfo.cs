using DeepSigma.SystemDiagnostics.Enums;

namespace DeepSigma.SystemDiagnostics.Models;

public sealed record CpuInfo(
    string Name,
    string Vendor,
    CpuArchitecture Architecture,
    int LogicalCores,
    int? PhysicalCores,
    double? MaxClockMHz);
