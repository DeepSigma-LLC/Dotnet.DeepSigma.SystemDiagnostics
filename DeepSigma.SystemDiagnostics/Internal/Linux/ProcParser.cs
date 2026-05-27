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
        catch
        {
        }
        return dict;
    }

    public static (string Name, string Vendor, int? PhysicalCores, double? MaxClockMHz) ParseCpuInfo()
    {
        string name = string.Empty;
        string vendor = string.Empty;
        double? maxClockMHz = null;
        var coreIds = new HashSet<string>(StringComparer.Ordinal);
        string? currentPhysicalId = null;
        var physicalIds = new HashSet<string>(StringComparer.Ordinal);

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
                        physicalIds.Add(value);
                        break;
                    case "core id":
                        coreIds.Add($"{currentPhysicalId ?? "0"}:{value}");
                        break;
                    case "cpu MHz":
                        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var mhz))
                        {
                            if (!maxClockMHz.HasValue || mhz > maxClockMHz.Value)
                                maxClockMHz = mhz;
                        }
                        break;
                }
            }
        }
        catch
        {
        }

        int? physicalCores = coreIds.Count > 0 ? coreIds.Count : null;

        return (
            string.IsNullOrEmpty(name) ? "Unknown" : name,
            string.IsNullOrEmpty(vendor) ? "Unknown" : vendor,
            physicalCores,
            maxClockMHz);
    }

    public static (ulong TotalBytes, ulong? AvailableBytes) ParseMemInfo()
    {
        ulong total = 0;
        ulong? available = null;

        var values = ReadKeyValueFile("/proc/meminfo", ':');
        if (values.TryGetValue("MemTotal", out var totalStr))
            total = ParseKbValue(totalStr);
        if (values.TryGetValue("MemAvailable", out var availStr))
            available = ParseKbValue(availStr);

        return (total, available);
    }

    private static ulong ParseKbValue(string raw)
    {
        var trimmed = raw.Trim();
        if (trimmed.EndsWith(" kB", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed[..^3].Trim();
        return ulong.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var kb)
            ? kb * 1024UL
            : 0UL;
    }
}
