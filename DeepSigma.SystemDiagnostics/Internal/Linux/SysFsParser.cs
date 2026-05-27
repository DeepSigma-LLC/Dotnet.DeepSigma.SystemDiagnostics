using System.Globalization;
using System.Runtime.Versioning;
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
        catch
        {
        }

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
        catch
        {
            return null;
        }
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
