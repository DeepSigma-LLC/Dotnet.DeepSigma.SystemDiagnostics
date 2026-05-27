# DeepSigma.SystemDiagnostics

Cross-platform (Windows + Linux) system diagnostics for .NET. Inspect the host OS, CPU, memory, drives, network adapters, and GPUs at runtime and pivot application logic on the results.

## Install

```
dotnet add package DeepSigma.SystemDiagnostics
```

## Usage

```csharp
using DeepSigma.SystemDiagnostics;
using DeepSigma.SystemDiagnostics.Enums;
using DeepSigma.SystemDiagnostics.Formatting;

// Pivot on OS family
if (SystemDiagnostics.CurrentOS == OSFamily.Windows)
{
    // Windows-specific path
}

// Individual probes
var cpu = SystemDiagnostics.GetCpu();
var memory = SystemDiagnostics.GetMemory();
Console.WriteLine($"{cpu.Name} - {cpu.LogicalCores} logical cores, {memory.TotalBytes / 1024 / 1024 / 1024} GB RAM");

// Full snapshot bundled into one record
var snapshot = SystemDiagnostics.GetSnapshot();
Console.WriteLine(snapshot.ToReadableString());
```

## What's collected

| Category | Source |
|---|---|
| OS family / version / architecture | `RuntimeInformation`, `Environment` |
| CPU name, vendor, cores, clock | WMI `Win32_Processor` (Windows) / `/proc/cpuinfo` (Linux) |
| Memory total / available | WMI `Win32_OperatingSystem` (Windows) / `/proc/meminfo` (Linux) |
| Drives | `System.IO.DriveInfo` |
| Network adapters | `System.Net.NetworkInformation.NetworkInterface` |
| GPUs | WMI `Win32_VideoController` (Windows) / `/sys/class/drm` (Linux) |

## Notes

- All result types are immutable `sealed record`s — safe to store and share.
- OS, CPU, and GPU values are cached for the process lifetime. Memory, drives, and network adapters are re-queried on every call.
- On unsupported platforms (e.g. macOS), the library returns best-effort values from built-in .NET APIs without throwing.

## License

See [LICENSE](LICENSE).
