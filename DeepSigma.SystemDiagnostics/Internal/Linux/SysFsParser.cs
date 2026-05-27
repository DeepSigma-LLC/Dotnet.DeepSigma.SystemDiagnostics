using System.Globalization;
using System.Runtime.Versioning;
using DeepSigma.SystemDiagnostics.Enums;
using DeepSigma.SystemDiagnostics.Models;

namespace DeepSigma.SystemDiagnostics.Internal.Linux;

[SupportedOSPlatform("linux")]
internal static class SysFsParser
{
    public static IReadOnlyList<GpuInfo> EnumerateGpus()
    {
        var result = new List<GpuInfo>();
        const string drmRoot = "/sys/class/drm";
        if (!Directory.Exists(drmRoot)) return result;

        try
        {
            foreach (var dir in Directory.GetDirectories(drmRoot, "card*"))
            {
                var leaf = Path.GetFileName(dir);
                if (leaf.Contains('-')) continue;

                var devicePath = Path.Combine(dir, "device");
                if (!Directory.Exists(devicePath)) continue;

                string? vendorId = ReadIdFile(Path.Combine(devicePath, "vendor"));
                string? deviceId = ReadIdFile(Path.Combine(devicePath, "device"));

                var vendor = MapVendor(vendorId);
                var name = deviceId is not null
                    ? $"{vendor ?? "GPU"} {deviceId}"
                    : leaf;

                result.Add(new GpuInfo(
                    Name: name,
                    Vendor: vendor,
                    DriverVersion: null,
                    AdapterRamBytes: null));
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }

        return result;
    }

    private static string? ReadIdFile(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            var raw = File.ReadAllText(path).Trim();
            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                raw = raw[2..];
            return ushort.TryParse(raw, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var id)
                ? id.ToString("X4", CultureInfo.InvariantCulture)
                : null;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    public static IReadOnlyList<BatteryInfo> EnumerateBatteries()
    {
        const string root = "/sys/class/power_supply";
        if (!Directory.Exists(root)) return Array.Empty<BatteryInfo>();

        bool? acOnline = null;
        var batteryDirs = new List<string>();

        try
        {
            foreach (var dir in Directory.GetDirectories(root))
            {
                var type = ReadTrim(Path.Combine(dir, "type"));
                if (string.Equals(type, "Battery", StringComparison.OrdinalIgnoreCase))
                {
                    batteryDirs.Add(dir);
                }
                else if (string.Equals(type, "Mains", StringComparison.OrdinalIgnoreCase))
                {
                    var online = ReadTrim(Path.Combine(dir, "online"));
                    if (online == "1") acOnline = true;
                    else if (online == "0") acOnline ??= false;
                }
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }

        if (batteryDirs.Count == 0) return Array.Empty<BatteryInfo>();

        var result = new List<BatteryInfo>();
        foreach (var dir in batteryDirs)
        {
            var name = Path.GetFileName(dir);
            int? percent = null;
            var capacityStr = ReadTrim(Path.Combine(dir, "capacity"));
            if (int.TryParse(capacityStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pct))
                percent = Math.Clamp(pct, 0, 100);

            var statusStr = ReadTrim(Path.Combine(dir, "status"));
            var status = BatteryStatusMapper.FromLinuxStatusFile(statusStr);

            bool isOnAc = acOnline ?? (status != BatteryStatus.Discharging && status != BatteryStatus.Unknown);

            result.Add(new BatteryInfo(
                Name: name,
                ChargePercent: percent,
                Status: status,
                IsOnAcPower: isOnAc));
        }
        return result;
    }

    public static IReadOnlyList<TemperatureReading> EnumerateTemperatures()
    {
        const string root = "/sys/class/hwmon";
        if (!Directory.Exists(root)) return Array.Empty<TemperatureReading>();

        var result = new List<TemperatureReading>();
        try
        {
            foreach (var chipDir in Directory.GetDirectories(root))
            {
                var chipName = ReadTrim(Path.Combine(chipDir, "name")) ?? Path.GetFileName(chipDir);

                string[] inputFiles;
                try
                {
                    inputFiles = Directory.GetFiles(chipDir, "temp*_input");
                }
                catch (IOException) { continue; }
                catch (UnauthorizedAccessException) { continue; }

                foreach (var inputFile in inputFiles)
                {
                    var leaf = Path.GetFileName(inputFile);
                    var prefix = leaf[..^"_input".Length];

                    var celsius = ReadMilliC(inputFile);
                    if (!celsius.HasValue) continue;

                    var label = ReadTrim(Path.Combine(chipDir, $"{prefix}_label"));
                    var max = ReadMilliC(Path.Combine(chipDir, $"{prefix}_max"));
                    var crit = ReadMilliC(Path.Combine(chipDir, $"{prefix}_crit"));

                    result.Add(new TemperatureReading(
                        ChipName: chipName,
                        Label: string.IsNullOrEmpty(label) ? null : label,
                        Celsius: celsius.Value,
                        MaxCelsius: max,
                        CritCelsius: crit));
                }
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }

        return result;
    }

    private static double? ReadMilliC(string path)
    {
        var raw = ReadTrim(path);
        if (string.IsNullOrEmpty(raw)) return null;
        return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var millis)
            ? millis / 1000.0
            : null;
    }

    private static string? ReadTrim(string path)
    {
        try
        {
            return File.Exists(path) ? File.ReadAllText(path).Trim() : null;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    private static string? MapVendor(string? vendorId) => vendorId switch
    {
        "10DE" => "NVIDIA",
        "1002" => "AMD",
        "8086" => "Intel",
        "1AF4" => "Red Hat (Virtio)",
        "15AD" => "VMware",
        "1414" => "Microsoft",
        null => null,
        _ => $"PCI {vendorId}",
    };
}
