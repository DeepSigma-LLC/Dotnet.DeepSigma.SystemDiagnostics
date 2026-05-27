using System.Globalization;
using System.Runtime.Versioning;

namespace DeepSigma.SystemDiagnostics.Internal.Linux;

[SupportedOSPlatform("linux")]
internal static class ProcParser
{
    public static Dictionary<string, string> ReadKeyValueFile(string path, char separator)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var line in File.ReadAllLines(path))
            {
                var idx = line.IndexOf(separator);
                if (idx <= 0) continue;
                var key = line[..idx].Trim();
                var value = line[(idx + 1)..].Trim();
                if (!dict.ContainsKey(key))
                    dict[key] = value;
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return dict;
    }

    public static (string Name, string Vendor, int? PhysicalCores) ParseCpuInfo()
    {
        string name = string.Empty;
        string vendor = string.Empty;
        var coreIds = new HashSet<string>(StringComparer.Ordinal);
        string? currentPhysicalId = null;

        try
        {
            foreach (var line in File.ReadAllLines("/proc/cpuinfo"))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var idx = line.IndexOf(':');
                if (idx < 0) continue;
                var key = line[..idx].Trim();
                var value = line[(idx + 1)..].Trim();

                switch (key)
                {
                    case "model name" when string.IsNullOrEmpty(name):
                        name = value;
                        break;
                    case "vendor_id" when string.IsNullOrEmpty(vendor):
                        vendor = value;
                        break;
                    case "physical id":
                        currentPhysicalId = value;
                        break;
                    case "core id":
                        coreIds.Add($"{currentPhysicalId ?? "0"}:{value}");
                        break;
                }
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }

        int? physicalCores = coreIds.Count > 0 ? coreIds.Count : null;

        return (
            string.IsNullOrEmpty(name) ? "Unknown" : name,
            string.IsNullOrEmpty(vendor) ? "Unknown" : vendor,
            physicalCores);
    }

    // Reads the highest cpuinfo_max_freq across all CPUs. Returns true max clock (boost),
    // not the current frequency. /proc/cpuinfo's "cpu MHz" reports the current frequency
    // at sample time, which is misleading as a "max clock" value.
    public static double? ReadMaxClockMHz()
    {
        const string root = "/sys/devices/system/cpu";
        if (!Directory.Exists(root)) return null;

        double? maxMHz = null;
        try
        {
            foreach (var cpuDir in Directory.GetDirectories(root, "cpu[0-9]*"))
            {
                var path = Path.Combine(cpuDir, "cpufreq", "cpuinfo_max_freq");
                if (!File.Exists(path)) continue;
                string raw;
                try
                {
                    raw = File.ReadAllText(path).Trim();
                }
                catch (IOException) { continue; }
                catch (UnauthorizedAccessException) { continue; }

                if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var kHz))
                {
                    var mhz = kHz / 1000.0;
                    if (!maxMHz.HasValue || mhz > maxMHz.Value)
                        maxMHz = mhz;
                }
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }

        return maxMHz;
    }

    public static (ulong TotalBytes, ulong? AvailableBytes) ParseMemInfo()
    {
        ulong total = 0;
        ulong? available = null;

        var values = ReadKeyValueFile("/proc/meminfo", ':');
        if (values.TryGetValue("MemTotal", out var totalStr))
            total = ParseKbValue(totalStr) ?? 0;
        if (values.TryGetValue("MemAvailable", out var availStr))
            available = ParseKbValue(availStr);

        return (total, available);
    }

    private static ulong? ParseKbValue(string raw)
    {
        var trimmed = raw.Trim();
        if (trimmed.EndsWith(" kB", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed[..^3].Trim();
        return ulong.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var kb)
            ? kb * 1024UL
            : null;
    }
}
