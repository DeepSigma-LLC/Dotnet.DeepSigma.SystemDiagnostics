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
        var family = DetectFamily();
        return new OperatingSystemInfo(
            Family: family,
            Description: RuntimeInformation.OSDescription,
            Version: FormatVersion(family, Environment.OSVersion.Version),
            MachineName: Environment.MachineName,
            UserName: Environment.UserName,
            Is64Bit: Environment.Is64BitOperatingSystem);
    }

    // The Environment.OSVersion.Version on Windows reports the NT version (10.0.x) for both
    // Windows 10 and Windows 11. The consumer-facing convention is that build >= 22000 == Win11.
    // Translate so the Version field reflects what users expect; the raw NT description is still
    // available via OperatingSystemInfo.Description.
    private static string FormatVersion(OSFamily family, Version v)
    {
        if (family == OSFamily.Windows && v.Major == 10 && v.Build >= 22000)
            return $"11.0.{v.Build}.{v.Revision}";
        return v.ToString();
    }

    public abstract CpuInfo GetCpu();

    public abstract MemoryInfo GetMemory();

    public virtual IReadOnlyList<DriveInfoRecord> GetDrives()
    {
        var result = new List<DriveInfoRecord>();

        IoDriveInfo[] drives;
        try
        {
            drives = IoDriveInfo.GetDrives();
        }
        catch (IOException) { return result; }
        catch (UnauthorizedAccessException) { return result; }

        foreach (var drive in drives)
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
                catch (IOException) { ready = false; }
                catch (UnauthorizedAccessException) { ready = false; }
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

        NetworkInterface[] nics;
        try
        {
            nics = NetworkInterface.GetAllNetworkInterfaces();
        }
        catch (NetworkInformationException) { return result; }
        catch (PlatformNotSupportedException) { return result; }

        foreach (var nic in nics)
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
            catch (NetworkInformationException) { }
            catch (PlatformNotSupportedException) { }

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

    public abstract IReadOnlyList<BatteryInfo> GetBatteries();

    public abstract IReadOnlyList<TemperatureReading> GetTemperatures();

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
