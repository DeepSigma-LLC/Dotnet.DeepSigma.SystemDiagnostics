namespace DeepSigma.SystemDiagnostics.Models;

/// <summary>
/// Aggregate snapshot of every category the library probes. Construct via
/// <see cref="SystemDiagnostics.GetSnapshot"/>.
/// </summary>
/// <remarks>
/// The snapshot is not atomic: each category is queried in turn. <see cref="CapturedAt"/>
/// is the UTC timestamp at the start of the snapshot. Total cost is order-of-magnitude 100 ms.
/// </remarks>
/// <param name="CapturedAt">UTC time at which the snapshot began.</param>
/// <param name="OperatingSystem">OS identity (cached for process lifetime).</param>
/// <param name="Cpu">CPU description (cached for process lifetime).</param>
/// <param name="Memory">Memory totals at capture time.</param>
/// <param name="Drives">Mounted storage volumes at capture time. On Linux, pseudo-filesystems are filtered out.</param>
/// <param name="NetworkAdapters">Curated network adapter list (filter drivers and tunnels excluded).</param>
/// <param name="Gpus">Detected graphics adapters (cached for process lifetime).</param>
/// <param name="Temperatures">Temperature readings (Linux only; empty list on Windows).</param>
public sealed record SystemSnapshot(
    DateTimeOffset CapturedAt,
    OperatingSystemInfo OperatingSystem,
    CpuInfo Cpu,
    MemoryInfo Memory,
    IReadOnlyList<DriveInfoRecord> Drives,
    IReadOnlyList<NetworkAdapterInfo> NetworkAdapters,
    IReadOnlyList<GpuInfo> Gpus,
    IReadOnlyList<TemperatureReading> Temperatures);
