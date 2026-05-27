using System.Globalization;
using System.Text;
using DeepSigma.SystemDiagnostics.Models;

namespace DeepSigma.SystemDiagnostics.Formatting;

public static class SnapshotFormatter
{
    public static string ToReadableString(this SystemSnapshot snapshot)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== System Snapshot (captured {snapshot.CapturedAt:u}) ===");
        sb.AppendLine();

        sb.AppendLine("-- Operating System --");
        var os = snapshot.OperatingSystem;
        sb.AppendLine($"  Family:      {os.Family}");
        sb.AppendLine($"  Description: {os.Description}");
        sb.AppendLine($"  Version:     {os.Version}");
        sb.AppendLine($"  Machine:     {os.MachineName}");
        sb.AppendLine($"  User:        {os.UserName}");
        sb.AppendLine($"  64-bit:      {os.Is64Bit}");
        sb.AppendLine();

        sb.AppendLine("-- CPU --");
        var cpu = snapshot.Cpu;
        sb.AppendLine($"  Name:           {cpu.Name}");
        sb.AppendLine($"  Vendor:         {cpu.Vendor}");
        sb.AppendLine($"  Architecture:   {cpu.Architecture}");
        sb.AppendLine($"  Logical cores:  {cpu.LogicalCores}");
        sb.AppendLine($"  Physical cores: {cpu.PhysicalCores?.ToString(CultureInfo.InvariantCulture) ?? "(unknown)"}");
        sb.AppendLine($"  Max clock:      {(cpu.MaxClockMHz.HasValue ? $"{cpu.MaxClockMHz.Value:F0} MHz" : "(unknown)")}");
        sb.AppendLine();

        sb.AppendLine("-- Memory --");
        sb.AppendLine($"  Total:     {FormatBytes(snapshot.Memory.TotalBytes)}");
        sb.AppendLine($"  Available: {(snapshot.Memory.AvailableBytes.HasValue ? FormatBytes(snapshot.Memory.AvailableBytes.Value) : "(unknown)")}");
        sb.AppendLine();

        sb.AppendLine("-- Drives --");
        if (snapshot.Drives.Count == 0)
        {
            sb.AppendLine("  (none)");
        }
        else
        {
            foreach (var d in snapshot.Drives)
            {
                if (d.IsReady)
                {
                    sb.AppendLine(
                        $"  {d.Name,-12} {d.Kind,-10} {d.FileSystem ?? "-",-8} " +
                        $"{FormatBytes(d.AvailableBytes)} free of {FormatBytes(d.TotalBytes)}" +
                        (string.IsNullOrEmpty(d.VolumeLabel) ? "" : $"  [{d.VolumeLabel}]"));
                }
                else
                {
                    sb.AppendLine($"  {d.Name,-12} {d.Kind,-10} (not ready)");
                }
            }
        }
        sb.AppendLine();

        sb.AppendLine("-- Network adapters --");
        if (snapshot.NetworkAdapters.Count == 0)
        {
            sb.AppendLine("  (none)");
        }
        else
        {
            foreach (var n in snapshot.NetworkAdapters)
            {
                sb.AppendLine($"  {n.Name}  [{n.Type}]  {(n.IsUp ? "UP" : "DOWN")}");
                if (!string.IsNullOrEmpty(n.MacAddress))
                    sb.AppendLine($"    MAC:   {n.MacAddress}");
                if (n.SpeedBitsPerSecond > 0)
                    sb.AppendLine($"    Speed: {FormatBitsPerSecond(n.SpeedBitsPerSecond)}");
                if (n.IPv4Addresses.Count > 0)
                    sb.AppendLine($"    IPv4:  {string.Join(", ", n.IPv4Addresses)}");
                if (n.IPv6Addresses.Count > 0)
                    sb.AppendLine($"    IPv6:  {string.Join(", ", n.IPv6Addresses)}");
            }
        }
        sb.AppendLine();

        sb.AppendLine("-- GPUs --");
        if (snapshot.Gpus.Count == 0)
        {
            sb.AppendLine("  (none detected)");
        }
        else
        {
            foreach (var g in snapshot.Gpus)
            {
                sb.AppendLine($"  {g.Name}");
                if (!string.IsNullOrEmpty(g.Vendor))
                    sb.AppendLine($"    Vendor: {g.Vendor}");
                if (!string.IsNullOrEmpty(g.DriverVersion))
                    sb.AppendLine($"    Driver: {g.DriverVersion}");
                if (g.AdapterRamBytes.HasValue && g.AdapterRamBytes.Value > 0)
                    sb.AppendLine($"    VRAM:   {FormatBytes(g.AdapterRamBytes.Value)}");
            }
        }

        return sb.ToString();
    }

    public static string FormatBytes(ulong bytes)
    {
        if (bytes == 0) return "0 B";
        string[] units = ["B", "KiB", "MiB", "GiB", "TiB", "PiB"];
        double value = bytes;
        int unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", value, units[unit]);
    }

    private static string FormatBitsPerSecond(long bps)
    {
        if (bps <= 0) return "0 bps";
        string[] units = ["bps", "Kbps", "Mbps", "Gbps", "Tbps"];
        double value = bps;
        int unit = 0;
        while (value >= 1000 && unit < units.Length - 1)
        {
            value /= 1000;
            unit++;
        }
        return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", value, units[unit]);
    }
}
