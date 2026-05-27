using System.Runtime.Versioning;
using DeepSigma.SystemDiagnostics.Models;

namespace DeepSigma.SystemDiagnostics.Internal.Linux;

[SupportedOSPlatform("linux")]
internal sealed class LinuxSystemInfoProvider : SystemInfoProviderBase
{
    // Linux's DriveInfo.GetDrives() returns every mount the kernel knows about, including
    // pseudo-filesystems like /proc, /sys, cgroupfs, etc. Filter them out so consumers see
    // only mounts that represent real storage.
    private static readonly HashSet<string> PseudoFilesystems = new(StringComparer.OrdinalIgnoreCase)
    {
        "proc", "sysfs", "devtmpfs", "devpts", "debugfs", "tracefs",
        "cgroup", "cgroup2", "configfs", "securityfs", "fusectl", "mqueue",
        "pstore", "rpc_pipefs", "autofs", "hugetlbfs", "bpf", "binfmt_misc",
        "ramfs", "nsfs", "fuse.gvfsd-fuse", "fuse.portal", "efivarfs",
    };

    public override IReadOnlyList<DriveInfoRecord> GetDrives()
    {
        var raw = base.GetDrives();
        var filtered = new List<DriveInfoRecord>(raw.Count);
        foreach (var drive in raw)
        {
            if (drive.FileSystem is not null && PseudoFilesystems.Contains(drive.FileSystem))
                continue;
            filtered.Add(drive);
        }
        return filtered;
    }

    public override CpuInfo GetCpu()
    {
        var (name, vendor, physicalCores) = ProcParser.ParseCpuInfo();
        return new CpuInfo(
            Name: name,
            Vendor: vendor,
            Architecture: CurrentOSArchitecture(),
            LogicalCores: LogicalCoreCount(),
            PhysicalCores: physicalCores,
            MaxClockMHz: ProcParser.ReadMaxClockMHz());
    }

    public override MemoryInfo GetMemory()
    {
        var (total, available) = ProcParser.ParseMemInfo();
        return new MemoryInfo(total, available);
    }

    public override IReadOnlyList<GpuInfo> GetGpus() => SysFsParser.EnumerateGpus();

    public override IReadOnlyList<BatteryInfo> GetBatteries() => SysFsParser.EnumerateBatteries();

    public override IReadOnlyList<TemperatureReading> GetTemperatures() =>
        SysFsParser.EnumerateTemperatures();
}
