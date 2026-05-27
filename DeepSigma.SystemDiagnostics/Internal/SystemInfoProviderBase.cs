using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using DeepSigma.SystemDiagnostics.Enums;
using DeepSigma.SystemDiagnostics.Models;
using IoDriveInfo = System.IO.DriveInfo;

namespace DeepSigma.SystemDiagnostics.Internal;

internal abstract class SystemInfoProviderBase : ISystemInfoProvider
{
    public virtual OperatingSystemInfo GetOperatingSystem()
    {
        return new OperatingSystemInfo(
            Family: DetectFamily(),
            Description: RuntimeInformation.OSDescription,
            Version: Environment.OSVersion.Version.ToString(),
            MachineName: Environment.MachineName,
            UserName: Environment.UserName,
            Is64Bit: Environment.Is64BitOperatingSystem);
    }

    public abstract CpuInfo GetCpu();

    public abstract MemoryInfo GetMemory();

    public virtual IReadOnlyList<DriveInfoRecord> GetDrives()
    {
        var result = new List<DriveInfoRecord>();
        foreach (var drive in IoDriveInfo.GetDrives())
        {
            ulong total = 0, free = 0;
            string? label = null, fs = null;
            var ready = drive.IsReady;
            if (ready)
            {
                try
                {
                    total = (ulong)drive.TotalSize;
                    free = (ulong)drive.AvailableFreeSpace;
                    label = drive.VolumeLabel;
                    fs = drive.DriveFormat;
                }
                catch
                {
                    ready = false;
                }
            }

            result.Add(new DriveInfoRecord(
                Name: drive.Name,
                VolumeLabel: label,
                FileSystem: fs,
                Kind: MapDriveKind(drive.DriveType),
                TotalBytes: total,
                AvailableBytes: free,
                IsReady: ready));
        }
        return result;
    }

    public virtual IReadOnlyList<NetworkAdapterInfo> GetNetworkAdapters()
    {
        var result = new List<NetworkAdapterInfo>();
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            var ipv4 = new List<string>();
            var ipv6 = new List<string>();
            try
            {
                foreach (var addr in nic.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        ipv4.Add(addr.Address.ToString());
                    else if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        ipv6.Add(addr.Address.ToString());
                }
            }
            catch
            {
            }

            result.Add(new NetworkAdapterInfo(
                Id: nic.Id,
                Name: nic.Name,
                Description: nic.Description,
                MacAddress: FormatMac(nic.GetPhysicalAddress()),
                Type: nic.NetworkInterfaceType.ToString(),
                IsUp: nic.OperationalStatus == OperationalStatus.Up,
                SpeedBitsPerSecond: nic.Speed,
                IPv4Addresses: ipv4,
                IPv6Addresses: ipv6));
        }
        return result;
    }

    public abstract IReadOnlyList<GpuInfo> GetGpus();

    protected static int LogicalCoreCount() => Environment.ProcessorCount;

    protected static CpuArchitecture MapArchitecture(Architecture arch) => arch switch
    {
        Architecture.X86 => CpuArchitecture.X86,
        Architecture.X64 => CpuArchitecture.X64,
        Architecture.Arm => CpuArchitecture.Arm,
        Architecture.Arm64 => CpuArchitecture.Arm64,
        _ => CpuArchitecture.Unknown,
    };

    protected static CpuArchitecture CurrentOSArchitecture() =>
        MapArchitecture(RuntimeInformation.OSArchitecture);

    private static OSFamily DetectFamily()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OSFamily.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OSFamily.Linux;
        return OSFamily.Unknown;
    }

    private static DriveKind MapDriveKind(System.IO.DriveType type) => type switch
    {
        System.IO.DriveType.Fixed => DriveKind.Fixed,
        System.IO.DriveType.Removable => DriveKind.Removable,
        System.IO.DriveType.Network => DriveKind.Network,
        System.IO.DriveType.Ram => DriveKind.Ram,
        System.IO.DriveType.CDRom => DriveKind.CdRom,
        _ => DriveKind.Unknown,
    };

    private static string FormatMac(PhysicalAddress mac)
    {
        var bytes = mac.GetAddressBytes();
        if (bytes.Length == 0) return string.Empty;
        return string.Join(":", bytes.Select(b => b.ToString("X2")));
    }
}
