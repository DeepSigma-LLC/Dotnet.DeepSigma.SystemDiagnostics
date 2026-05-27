using DeepSigma.SystemDiagnostics.Models;

namespace DeepSigma.SystemDiagnostics.Internal;

internal sealed class FallbackSystemInfoProvider : SystemInfoProviderBase
{
    public override CpuInfo GetCpu() => new(
        Name: "Unknown",
        Vendor: "Unknown",
        Architecture: CurrentOSArchitecture(),
        LogicalCores: LogicalCoreCount(),
        PhysicalCores: null,
        MaxClockMHz: null);

    public override MemoryInfo GetMemory() => new(TotalBytes: 0, AvailableBytes: null);

    public override IReadOnlyList<GpuInfo> GetGpus() => Array.Empty<GpuInfo>();
}
