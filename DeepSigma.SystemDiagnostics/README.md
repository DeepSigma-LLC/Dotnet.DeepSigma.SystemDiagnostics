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

// Batteries are queried separately (not in the snapshot)
foreach (var battery in SystemDiagnostics.GetBatteries())
{
    if (!battery.IsOnAcPower && battery.ChargePercent < 20)
        Console.WriteLine($"Low battery: {battery.ChargePercent}%");
}

// Network adapters are filtered by default to exclude NDIS filter drivers,
// QoS schedulers, tunnel pseudo-interfaces, and other noise. Use the raw
// list if you need every adapter the OS reports:
var allAdapters = SystemDiagnostics.GetAllNetworkAdapters();
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
| Batteries | WMI `Win32_Battery` (Windows) / `/sys/class/power_supply` (Linux) |
| Temperatures | `/sys/class/hwmon` (Linux only — see note below) |

## Notes

- All result types are immutable `sealed record`s — safe to store and share.
- OS, CPU, and GPU values are cached for the process lifetime. Memory, drives, network adapters, batteries, and temperatures are re-queried on every call.
- On unsupported platforms (e.g. macOS), the library returns best-effort values from built-in .NET APIs without throwing.

### Temperature sensors are Linux-only

`GetTemperatures()` reads `/sys/class/hwmon/hwmon*/temp*_input` on Linux — no root required, reliable on every modern kernel. On Windows the call returns an empty list by design. There is no practical user-mode API on Windows for chip temperatures:

- WMI `MSAcpi_ThermalZoneTemperature` requires elevation and returns junk or empty on most consumer hardware.
- The only reliable Windows sources (LibreHardwareMonitor, OpenHardwareMonitor) ship unsigned kernel drivers that require admin install and would force this package to demand elevation.

If you need Windows sensor data, integrate LibreHardwareMonitorLib directly in your application — it is intentionally not a dependency of this package.

**WSL2 caveat:** under WSL2 the VM kernel does not expose host hardware sensors, so `/sys/class/hwmon` is typically empty even though the platform reports as Linux. Run on bare-metal Linux (or a Linux VM with PCI passthrough) to see real readings.

## License

See [LICENSE](LICENSE).
