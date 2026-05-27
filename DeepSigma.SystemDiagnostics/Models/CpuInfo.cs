using DeepSigma.SystemDiagnostics.Enums;

namespace DeepSigma.SystemDiagnostics.Models;

/// <summary>
/// Description of the host CPU. Aggregated across all sockets on multi-CPU systems.
/// </summary>
/// <param name="Name">Marketing name, e.g. <c>"11th Gen Intel(R) Core(TM) i7-11800H @ 2.30GHz"</c>.
/// <c>"Unknown"</c> when not available.</param>
/// <param name="Vendor">Vendor string, e.g. <c>"GenuineIntel"</c>, <c>"AuthenticAMD"</c>.
/// <c>"Unknown"</c> when not available.</param>
/// <param name="Architecture">Instruction-set architecture of the OS.</param>
/// <param name="LogicalCores">Logical processor count (<c>Environment.ProcessorCount</c>).
/// Includes hyperthreading.</param>
/// <param name="PhysicalCores">Physical core count summed across all sockets, or <c>null</c>
/// if the OS does not expose it.</param>
/// <param name="MaxClockMHz">Maximum advertised clock frequency in MHz (boost clock on
/// supported hardware), or <c>null</c> if unavailable.</param>
public sealed record CpuInfo(
    string Name,
    string Vendor,
    CpuArchitecture Architecture,
    int LogicalCores,
    int? PhysicalCores,
    double? MaxClockMHz);
