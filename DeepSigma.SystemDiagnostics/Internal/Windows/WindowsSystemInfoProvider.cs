using System.Runtime.Versioning;
using DeepSigma.SystemDiagnostics.Enums;
using DeepSigma.SystemDiagnostics.Models;

namespace DeepSigma.SystemDiagnostics.Internal.Windows;

[SupportedOSPlatform("windows")]
internal sealed class WindowsSystemInfoProvider : SystemInfoProviderBase
{
    public override CpuInfo GetCpu()
    {
        string name = string.Empty;
        string vendor = string.Empty;
        int? physicalCores = null;
        double? maxClockMHz = null;
        int logicalCores = LogicalCoreCount();

        try
        {
            foreach (var obj in WmiHelper.Query(
                "SELECT Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed FROM Win32_Processor"))
            {
                using (obj)
                {
                    if (string.IsNullOrEmpty(name))
                        name = (WmiHelper.ReadString(obj, "Name") ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(vendor))
                        vendor = (WmiHelper.ReadString(obj, "Manufacturer") ?? string.Empty).Trim();

                    var cores = WmiHelper.Read<uint>(obj, "NumberOfCores");
                    if (cores.HasValue)
                        physicalCores = (physicalCores ?? 0) + (int)cores.Value;

                    var clock = WmiHelper.Read<uint>(obj, "MaxClockSpeed");
                    if (clock.HasValue && (!maxClockMHz.HasValue || clock.Value > maxClockMHz.Value))
                        maxClockMHz = clock.Value;
                }
                break;
            }
        }
        catch
        {
        }

        return new CpuInfo(
            Name: string.IsNullOrEmpty(name) ? "Unknown" : name,
            Vendor: string.IsNullOrEmpty(vendor) ? "Unknown" : vendor,
            Architecture: CurrentOSArchitecture(),
            LogicalCores: logicalCores,
            PhysicalCores: physicalCores,
            MaxClockMHz: maxClockMHz);
    }

    public override MemoryInfo GetMemory()
    {
        ulong totalKb = 0;
        ulong? freeKb = null;
        try
        {
            foreach (var obj in WmiHelper.Query(
                "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
            {
                using (obj)
                {
                    totalKb = WmiHelper.Read<ulong>(obj, "TotalVisibleMemorySize") ?? 0;
                    freeKb = WmiHelper.Read<ulong>(obj, "FreePhysicalMemory");
                }
                break;
            }
        }
        catch
        {
        }

        return new MemoryInfo(
            TotalBytes: totalKb * 1024,
            AvailableBytes: freeKb.HasValue ? freeKb.Value * 1024 : null);
    }

    public override IReadOnlyList<GpuInfo> GetGpus()
    {
        var result = new List<GpuInfo>();
        try
        {
            foreach (var obj in WmiHelper.Query(
                "SELECT Name, AdapterCompatibility, DriverVersion, AdapterRAM FROM Win32_VideoController"))
            {
                using (obj)
                {
                    var name = (WmiHelper.ReadString(obj, "Name") ?? "Unknown").Trim();
                    var vendor = WmiHelper.ReadString(obj, "AdapterCompatibility")?.Trim();
                    var driver = WmiHelper.ReadString(obj, "DriverVersion")?.Trim();
                    var ram = WmiHelper.Read<uint>(obj, "AdapterRAM");
                    result.Add(new GpuInfo(
                        Name: name,
                        Vendor: string.IsNullOrEmpty(vendor) ? null : vendor,
                        DriverVersion: string.IsNullOrEmpty(driver) ? null : driver,
                        AdapterRamBytes: ram.HasValue ? (ulong)ram.Value : null));
                }
            }
        }
        catch
        {
        }
        return result;
    }
}
