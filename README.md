# DeepSigma.SystemDiagnostics

Cross-platform (Windows + Linux) system diagnostics for .NET. Inspect the host OS, CPU, memory, drives, network adapters, GPUs, batteries, and temperatures at runtime and branch application logic on the results.

- Single static entry point — no DI, no configuration required.
- All result types are immutable `sealed record`s.
- Degrades gracefully: probes never throw at the public API surface; failed reads return empty lists or "Unknown" fields.
- One managed dependency (`System.Management`, used only inside the Windows code path). No native binaries, no kernel drivers, no elevation.

```
dotnet add package DeepSigma.SystemDiagnostics
```

```csharp
using DeepSigma.SystemDiagnostics;
using DeepSigma.SystemDiagnostics.Formatting;

var snapshot = SystemDiagnostics.GetSnapshot();
Console.WriteLine(snapshot.ToReadableString());
```

Full usage docs and examples: see [DeepSigma.SystemDiagnostics/README.md](DeepSigma.SystemDiagnostics/README.md) (this is what ships inside the NuGet package).

---

## Repository layout

```
Dotnet.DeepSigma.SystemDiagnostics/
├── DeepSigma.SystemDiagnostics/             # Library — published as a NuGet package
│   ├── Models/                              # Public immutable record types
│   ├── Enums/                               # Public enums (OSFamily, etc.)
│   ├── Formatting/                          # SnapshotFormatter extensions
│   ├── Internal/                            # Provider abstraction + per-OS readers
│   │   ├── Windows/                         # WMI-backed readers (System.Management)
│   │   └── Linux/                           # /proc + /sys parsers
│   ├── SystemDiagnostics.cs                 # Public static facade
│   └── README.md                            # Packed into the NuGet
├── DeepSigma.SystemDiagnostics.Demo/        # Console app — prints a full snapshot
├── DeepSigma.SystemDiagnostics.Test/        # xUnit v3 test project
├── LICENSE
└── README.md                                # You are here
```

## Building from source

Requires the .NET 10 SDK.

```bash
# Build the library
dotnet build DeepSigma.SystemDiagnostics/DeepSigma.SystemDiagnostics.csproj

# Run the demo
dotnet run --project DeepSigma.SystemDiagnostics.Demo

# Run the tests (62 tests covering hardware invariants + pure-logic units)
dotnet test DeepSigma.SystemDiagnostics.Test/DeepSigma.SystemDiagnostics.Test.csproj

# Produce a NuGet package locally
dotnet pack DeepSigma.SystemDiagnostics/DeepSigma.SystemDiagnostics.csproj -c Release
```

The demo output is what most consumers will use to verify the library works on their machine — it prints a populated snapshot of every category. Sample output is in the [package README](DeepSigma.SystemDiagnostics/README.md).

## What's collected

| Category | Source |
|---|---|
| OS family / version / architecture | `RuntimeInformation`, `Environment` |
| CPU name, vendor, cores, clock | WMI `Win32_Processor` (Windows) / `/proc/cpuinfo` + `cpuinfo_max_freq` (Linux) |
| Memory total / available | WMI `Win32_OperatingSystem` (Windows) / `/proc/meminfo` (Linux) |
| Drives | `System.IO.DriveInfo` (Linux pseudo-filesystems filtered out) |
| Network adapters | `System.Net.NetworkInformation.NetworkInterface` (filter drivers / tunnels excluded by default) |
| GPUs | WMI `Win32_VideoController` (Windows) / `/sys/class/drm` (Linux) |
| Batteries | WMI `Win32_Battery` (Windows) / `/sys/class/power_supply` (Linux) |
| Temperatures | `/sys/class/hwmon` (Linux only — Windows requires an elevated kernel driver) |

## Design constraints

- **Target frameworks:** `net10.0` only. No multi-targeting; consumers on older frameworks should pin to a future LTS-targeted version.
- **Supported OSes:** Windows + Linux. macOS and BSD currently fall through to a degraded provider that returns only what built-in .NET APIs expose.
- **No elevation.** Everything works from a standard user account. Categories that would require admin (Windows temperature sensors, full SMART data) are intentionally out of scope.
- **No external native dependencies.** Adding LibreHardwareMonitor or vendor SDKs would mean shipping unsigned kernel drivers and forcing elevation onto every consumer — see the temperature note in the package README.

## Project status

This is an early-stage library. The public API is stable in shape (records + a static facade) but the version is still `0.x` while edges get polished. Breaking changes will bump the minor version.

## License

[MIT](LICENSE).
