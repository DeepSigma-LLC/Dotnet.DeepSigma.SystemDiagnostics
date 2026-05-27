# DeepSigma.SystemDiagnostics

Cross-platform (Windows + Linux) system diagnostics for .NET. Inspect the host OS, CPU, memory, drives, network adapters, GPUs, batteries, and temperatures at runtime and branch application logic on the results.

- Single static entry point — no DI, no configuration required.
- All result types are immutable `sealed record`s.
- Degrades gracefully: probes never throw at the public API surface; failed reads return empty lists or "Unknown" fields.

## Getting started

### 1. Install

```
dotnet add package DeepSigma.SystemDiagnostics
```

Target framework: **net10.0**. Single managed dependency: `System.Management` (used only inside the Windows code path).

### 2. Take a snapshot

The fastest way to see everything is to capture and print a full snapshot:

```csharp
using DeepSigma.SystemDiagnostics;
using DeepSigma.SystemDiagnostics.Formatting;

var snapshot = SystemDiagnostics.GetSnapshot();
Console.WriteLine(snapshot.ToReadableString());
```

Sample output (Windows 11 laptop):

```
=== System Snapshot (captured 2026-05-27 19:12:40Z) ===

-- Operating System --
  Family:      Windows
  Description: Microsoft Windows 10.0.26100
  Version:     11.0.26100.0
  Machine:     BP_XPS
  User:        brend
  64-bit:      True

-- CPU --
  Name:           11th Gen Intel(R) Core(TM) i7-11800H @ 2.30GHz
  Vendor:         GenuineIntel
  Architecture:   X64
  Logical cores:  16
  Physical cores: 8
  Max clock:      2304 MHz

-- Memory --
  Total:     31.73 GiB
  Available: 7.34 GiB

-- Drives --
  C:\          Fixed      NTFS     1.27 TiB free of 1.84 TiB  [OS]

-- Network adapters --
  Wi-Fi  [Wireless80211]  UP
    MAC:   2C:6D:C1:93:3C:A3
    Speed: 245 Mbps
    IPv4:  192.168.0.12

-- GPUs --
  NVIDIA GeForce RTX 3050 Laptop GPU
    Vendor: NVIDIA
    Driver: 32.0.15.8195
```

### 3. Access individual categories

Each category has its own getter on `SystemDiagnostics`. The values returned are immutable records you can store, compare, and pass around freely.

```csharp
var cpu = SystemDiagnostics.GetCpu();
var memory = SystemDiagnostics.GetMemory();

Console.WriteLine($"{cpu.Name}");
Console.WriteLine($"{cpu.LogicalCores} logical / {cpu.PhysicalCores} physical cores");
Console.WriteLine($"{memory.TotalBytes / 1024 / 1024 / 1024} GiB RAM total");
```

## Examples by scenario

### Branch on operating system

```csharp
using DeepSigma.SystemDiagnostics;
using DeepSigma.SystemDiagnostics.Enums;

string configPath = SystemDiagnostics.CurrentOS switch
{
    OSFamily.Windows => @"C:\ProgramData\MyApp\config.json",
    OSFamily.Linux   => "/etc/myapp/config.json",
    _                => Path.Combine(AppContext.BaseDirectory, "config.json"),
};
```

### Adjust workload to available cores

```csharp
var cpu = SystemDiagnostics.GetCpu();
int workers = Math.Max(1, cpu.LogicalCores - 2);   // leave headroom for the OS
var parallelism = new ParallelOptions { MaxDegreeOfParallelism = workers };
```

### Pick an algorithm based on architecture

```csharp
using DeepSigma.SystemDiagnostics.Enums;

var cpu = SystemDiagnostics.GetCpu();
if (cpu.Architecture is CpuArchitecture.Arm64)
{
    // Use a NEON-accelerated path instead of AVX2
}
```

### Detect low memory before a big allocation

```csharp
var memory = SystemDiagnostics.GetMemory();
if (memory.AvailableBytes is ulong available && available < 2UL * 1024 * 1024 * 1024)
{
    logger.LogWarning("Less than 2 GiB available — falling back to streaming pipeline");
}
```

### Guard against low disk space

```csharp
foreach (var drive in SystemDiagnostics.GetDrives())
{
    if (!drive.IsReady) continue;
    var pctFree = (double)drive.AvailableBytes / drive.TotalBytes;
    if (pctFree < 0.05)
        logger.LogWarning("Drive {Name} is below 5% free ({Free} bytes)", drive.Name, drive.AvailableBytes);
}
```

### Low-battery handling on laptops

```csharp
foreach (var battery in SystemDiagnostics.GetBatteries())
{
    if (!battery.IsOnAcPower && battery.ChargePercent < 20)
    {
        // Save state, switch to a lower-power mode, etc.
        logger.LogWarning("Battery at {Pct}% and not on AC power", battery.ChargePercent);
    }
}
```

### Temperature-based throttling (Linux only)

```csharp
var hottest = SystemDiagnostics.GetTemperatures()
    .Where(t => t.Label?.StartsWith("Core") == true)
    .DefaultIfEmpty()
    .Max(t => t?.Celsius);

if (hottest > 90)
{
    // CPU is hot — back off the worker pool
}
```

### Build a structured log payload

```csharp
var snapshot = SystemDiagnostics.GetSnapshot();
logger.LogInformation("Host snapshot {@Snapshot}", snapshot);   // Serilog-style structured log
```

### Inspect all network adapters, including filter drivers

```csharp
// Default getter is curated (filter drivers, schedulers, tunnels removed)
var primary = SystemDiagnostics.GetNetworkAdapters();

// Raw list with everything the OS reports
var raw = SystemDiagnostics.GetAllNetworkAdapters();
Console.WriteLine($"{raw.Count} total adapters, {primary.Count} after curation");
```

## What's collected

| Category | Source |
|---|---|
| OS family / version / architecture | `RuntimeInformation`, `Environment` |
| CPU name, vendor, cores, clock | WMI `Win32_Processor` (Windows) / `/proc/cpuinfo` + `/sys/devices/system/cpu/.../cpuinfo_max_freq` (Linux) |
| Memory total / available | WMI `Win32_OperatingSystem` (Windows) / `/proc/meminfo` (Linux) |
| Drives | `System.IO.DriveInfo` (Linux pseudo-filesystems filtered out) |
| Network adapters | `System.Net.NetworkInformation.NetworkInterface` (filter drivers and tunnels excluded by default) |
| GPUs | WMI `Win32_VideoController` (Windows) / `/sys/class/drm` (Linux) |
| Batteries | WMI `Win32_Battery` (Windows) / `/sys/class/power_supply` (Linux) |
| Temperatures | `/sys/class/hwmon` (Linux only — see note below) |

## API surface

| Member | Cached? | Notes |
|---|---|---|
| `SystemDiagnostics.CurrentOS` | yes | Shortcut for `GetOperatingSystem().Family`. |
| `GetOperatingSystem()` | yes | OS identity. |
| `GetCpu()` | yes | CPU identity + core counts. |
| `GetMemory()` | no | Re-queried each call. |
| `GetDrives()` | no | Curated on Linux (pseudo-filesystems excluded). |
| `GetNetworkAdapters()` | no | Curated (filter drivers / tunnels removed). |
| `GetAllNetworkAdapters()` | no | Raw OS list, including filter-driver sub-instances. |
| `GetGpus()` | yes | Graphics adapters. |
| `GetBatteries()` | no | Charge changes over time. |
| `GetTemperatures()` | no | Linux only; empty list on Windows. |
| `GetSnapshot()` | n/a | Invokes each getter once; ~100 ms total. |

Cached values are computed lazily on first use and frozen for the rest of the process — they describe hardware/OS state that doesn't change at runtime. Live values are re-read on every call.

## Notes and caveats

- All result types are immutable `sealed record`s — safe to share between threads and use as dictionary keys.
- Probes are best-effort. A failed WMI query or unreadable `/sys` file yields an empty list or a record with "Unknown"/`null` fields rather than an exception.
- `OperatingSystemInfo.Version` is normalized on Windows 11 hosts (major version reads as `11`) even though the underlying NT version is `10.0.x`. The raw OS string remains in `Description`.

### Temperature sensors are Linux-only

`GetTemperatures()` reads `/sys/class/hwmon/hwmon*/temp*_input` on Linux — no root required, reliable on every modern kernel. On Windows the call returns an empty list by design. There is no practical user-mode API on Windows for chip temperatures:

- WMI `MSAcpi_ThermalZoneTemperature` requires elevation and returns junk or empty on most consumer hardware.
- The only reliable Windows sources (LibreHardwareMonitor, OpenHardwareMonitor) ship unsigned kernel drivers that require admin install and would force this package to demand elevation.

If you need Windows sensor data, integrate LibreHardwareMonitorLib directly in your application — it is intentionally not a dependency of this package.

**WSL2 caveat:** under WSL2 the VM kernel does not expose host hardware sensors, so `/sys/class/hwmon` is typically empty even though the platform reports as Linux. Run on bare-metal Linux (or a Linux VM with PCI passthrough) to see real readings.

## License

See [LICENSE](LICENSE).
