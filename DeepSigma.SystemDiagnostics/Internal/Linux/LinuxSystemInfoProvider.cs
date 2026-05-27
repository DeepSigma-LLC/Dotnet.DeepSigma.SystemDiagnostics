using System.Runtime.Versioning;
using DeepSigma.SystemDiagnostics.Models;

namespace DeepSigma.SystemDiagnostics.Internal.Linux;

[SupportedOSPlatform("linux")]
internal sealed class LinuxSystemInfoProvider : SystemInfoProviderBase
{
    public override CpuInfo GetCpu()
    {
        var (name, vendor, physicalCores, maxClockMHz) = ProcParser.ParseCpuInfo();
        return new CpuInfo(
            Name: name,
            Vendor: vendor,
            Architecture: CurrentOSArchitecture(),
            LogicalCores: LogicalCoreCount(),
            PhysicalCores: physicalCores,
            MaxClockMHz: maxClockMHz);
    }

    public override MemoryInfo GetMemory()
    {
        var (total, available) = ProcParser.ParseMemInfo();
        return new MemoryInfo(total, available);
    }

    public override IReadOnlyList<GpuInfo> GetGpus() => SysFsParser.EnumerateGpus();
}
