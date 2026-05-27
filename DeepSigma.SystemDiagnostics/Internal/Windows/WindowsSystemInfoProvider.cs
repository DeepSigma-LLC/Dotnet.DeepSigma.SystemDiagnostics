using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
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
                "SELECT Name, Manufacturer, NumberOfCores, MaxClockSpeed FROM Win32_Processor"))
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
            }
        }
        catch (ManagementException) { }
        catch (UnauthorizedAccessException) { }
        catch (COMException) { }

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
        catch (ManagementException) { }
        catch (UnauthorizedAccessException) { }
        catch (COMException) { }

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

                    // Win32_VideoController.AdapterRAM is uint32, so values are capped at 4 GiB.
                    // Modern GPUs with more VRAM report uint.MaxValue (or near it from wrap).
                    // Treat the saturated value as unknown rather than misleadingly displaying 4 GiB.
                    var ram = WmiHelper.Read<uint>(obj, "AdapterRAM");
                    ulong? ramBytes = ram switch
                    {
                        null => null,
                        0 => null,
                        uint.MaxValue => null,
                        _ => ram.Value,
                    };

                    result.Add(new GpuInfo(
                        Name: name,
                        Vendor: string.IsNullOrEmpty(vendor) ? null : vendor,
                        DriverVersion: string.IsNullOrEmpty(driver) ? null : driver,
                        AdapterRamBytes: ramBytes));
                }
            }
        }
        catch (ManagementException) { }
        catch (UnauthorizedAccessException) { }
        catch (COMException) { }
        return result;
    }

    public override IReadOnlyList<BatteryInfo> GetBatteries()
    {
        var result = new List<BatteryInfo>();
        try
        {
            foreach (var obj in WmiHelper.Query(
                "SELECT Name, BatteryStatus, EstimatedChargeRemaining FROM Win32_Battery"))
            {
                using (obj)
                {
                    var name = (WmiHelper.ReadString(obj, "Name") ?? "Battery").Trim();
                    var code = WmiHelper.Read<ushort>(obj, "BatteryStatus");
                    var charge = WmiHelper.Read<ushort>(obj, "EstimatedChargeRemaining");

                    var (status, onAc) = BatteryStatusMapper.FromWindowsCode(code);
                    result.Add(new BatteryInfo(
                        Name: string.IsNullOrEmpty(name) ? "Battery" : name,
                        ChargePercent: charge.HasValue ? (int)charge.Value : null,
                        Status: status,
                        IsOnAcPower: onAc));
                }
            }
        }
        catch (ManagementException) { }
        catch (UnauthorizedAccessException) { }
        catch (COMException) { }
        return result;
    }

    // Windows has no practical user-mode API for reading CPU/GPU/chipset temperatures.
    // WMI MSAcpi_ThermalZoneTemperature requires elevation and returns junk or empty on most
    // consumer hardware. The only reliable Windows sensor sources (LibreHardwareMonitor,
    // OpenHardwareMonitor, vendor SDKs) load unsigned kernel drivers and require admin install,
    // which would force this whole package to demand elevation. Intentionally returns empty.
    public override IReadOnlyList<TemperatureReading> GetTemperatures() =>
        Array.Empty<TemperatureReading>();
}
