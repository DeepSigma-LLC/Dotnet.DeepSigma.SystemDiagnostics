using DeepSigma.SystemDiagnostics.Enums;
using DeepSigma.SystemDiagnostics.Internal;
using DeepSigma.SystemDiagnostics.Models;

namespace DeepSigma.SystemDiagnostics;

/// <summary>
/// Entry point for inspecting the host machine: OS, CPU, memory, drives, network adapters,
/// GPUs, batteries, and temperatures. Results are immutable records suitable for branching
/// application logic on host capabilities.
/// </summary>
/// <remarks>
/// <para>
/// <b>Caching:</b> <see cref="GetOperatingSystem"/>, <see cref="GetCpu"/>, and
/// <see cref="GetGpus"/> are cached for the lifetime of the process — they describe
/// hardware/OS state that does not change while the process runs.
/// <see cref="GetMemory"/>, <see cref="GetDrives"/>, <see cref="GetNetworkAdapters"/>,
/// <see cref="GetBatteries"/>, and <see cref="GetTemperatures"/> are re-queried on
/// every call because their values change over time.
/// </para>
/// <para>
/// <b>Snapshot cost:</b> <see cref="GetSnapshot"/> invokes each live getter once.
/// On Windows this fires several WMI queries; on Linux it reads files under
/// <c>/proc</c> and <c>/sys</c>. Total cost is order-of-magnitude 100 ms.
/// <see cref="Models.SystemSnapshot.CapturedAt"/> records the start of the snapshot;
/// the snapshot is not atomic across categories.
/// </para>
/// <para>
/// <b>Failure mode:</b> probes degrade gracefully. On a WMI or file-system error,
/// the affected getter returns an empty list or a record populated with
/// "Unknown" / null fields rather than throwing.
/// </para>
/// </remarks>
public static class SystemDiagnostics
{
    private static readonly Lazy<ISystemInfoProvider> _provider =
        new(ProviderFactory.Create, isThreadSafe: true);

    private static readonly Lazy<OperatingSystemInfo> _os =
        new(() => _provider.Value.GetOperatingSystem(), isThreadSafe: true);

    private static readonly Lazy<CpuInfo> _cpu =
        new(() => _provider.Value.GetCpu(), isThreadSafe: true);

    private static readonly Lazy<IReadOnlyList<GpuInfo>> _gpus =
        new(() => _provider.Value.GetGpus(), isThreadSafe: true);

    public static OSFamily CurrentOS => _os.Value.Family;

    public static OperatingSystemInfo GetOperatingSystem() => _os.Value;

    public static CpuInfo GetCpu() => _cpu.Value;

    public static MemoryInfo GetMemory() => _provider.Value.GetMemory();

    public static IReadOnlyList<DriveInfoRecord> GetDrives() => _provider.Value.GetDrives();

    public static IReadOnlyList<NetworkAdapterInfo> GetNetworkAdapters() =>
        _provider.Value.GetNetworkAdapters()
            .Where(NetworkAdapterFilter.IsCurated)
            .ToList();

    public static IReadOnlyList<NetworkAdapterInfo> GetAllNetworkAdapters() =>
        _provider.Value.GetNetworkAdapters();

    public static IReadOnlyList<GpuInfo> GetGpus() => _gpus.Value;

    public static IReadOnlyList<BatteryInfo> GetBatteries() => _provider.Value.GetBatteries();

    public static IReadOnlyList<TemperatureReading> GetTemperatures() =>
        _provider.Value.GetTemperatures();

    public static SystemSnapshot GetSnapshot() => new(
        CapturedAt: DateTimeOffset.UtcNow,
        OperatingSystem: GetOperatingSystem(),
        Cpu: GetCpu(),
        Memory: GetMemory(),
        Drives: GetDrives(),
        NetworkAdapters: GetNetworkAdapters(),
        Gpus: GetGpus(),
        Temperatures: GetTemperatures());
}
