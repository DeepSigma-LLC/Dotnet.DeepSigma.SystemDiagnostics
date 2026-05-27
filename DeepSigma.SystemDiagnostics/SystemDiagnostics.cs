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

    /// <summary>
    /// High-level operating-system family of the host. Cached for the process lifetime.
    /// Use this for ergonomic switch/if logic on Windows / Linux / Unknown.
    /// </summary>
    public static OSFamily CurrentOS => _os.Value.Family;

    /// <summary>
    /// Returns the host operating-system identity. Cached for the process lifetime.
    /// </summary>
    public static OperatingSystemInfo GetOperatingSystem() => _os.Value;

    /// <summary>
    /// Returns CPU vendor, name, core counts, architecture, and max clock.
    /// Cached for the process lifetime.
    /// </summary>
    public static CpuInfo GetCpu() => _cpu.Value;

    /// <summary>
    /// Returns physical memory totals at the moment of the call. Re-queried each invocation.
    /// </summary>
    public static MemoryInfo GetMemory() => _provider.Value.GetMemory();

    /// <summary>
    /// Returns the host's mounted storage volumes at the moment of the call.
    /// On Linux, pseudo-filesystems (<c>proc</c>, <c>sysfs</c>, <c>cgroup</c>, etc.) are filtered out.
    /// </summary>
    public static IReadOnlyList<DriveInfoRecord> GetDrives() => _provider.Value.GetDrives();

    /// <summary>
    /// Returns the curated network adapter list — NDIS filter drivers, QoS schedulers, and
    /// tunnel pseudo-interfaces are excluded. Use <see cref="GetAllNetworkAdapters"/> for the
    /// raw OS list.
    /// </summary>
    public static IReadOnlyList<NetworkAdapterInfo> GetNetworkAdapters() =>
        _provider.Value.GetNetworkAdapters()
            .Where(NetworkAdapterFilter.IsCurated)
            .ToList();

    /// <summary>
    /// Returns every network interface the OS reports, including filter-driver instances,
    /// QoS schedulers, and tunnels. Useful for low-level diagnostics; for most callers
    /// <see cref="GetNetworkAdapters"/> is more appropriate.
    /// </summary>
    public static IReadOnlyList<NetworkAdapterInfo> GetAllNetworkAdapters() =>
        _provider.Value.GetNetworkAdapters();

    /// <summary>
    /// Returns the host's graphics adapters. Cached for the process lifetime.
    /// </summary>
    public static IReadOnlyList<GpuInfo> GetGpus() => _gpus.Value;

    /// <summary>
    /// Returns the host's batteries. Empty on desktops or hosts with no battery hardware.
    /// Re-queried each invocation because charge level changes over time.
    /// </summary>
    public static IReadOnlyList<BatteryInfo> GetBatteries() => _provider.Value.GetBatteries();

    /// <summary>
    /// Returns the host's hardware temperature sensors.
    /// </summary>
    /// <remarks>
    /// Linux only. On Windows this always returns an empty list — there is no practical
    /// user-mode API for chip temperatures without an elevated kernel-driver dependency.
    /// Under WSL2 the VM kernel typically does not surface host sensors either.
    /// </remarks>
    public static IReadOnlyList<TemperatureReading> GetTemperatures() =>
        _provider.Value.GetTemperatures();

    /// <summary>
    /// Captures every category in a single immutable <see cref="SystemSnapshot"/> record.
    /// </summary>
    /// <remarks>
    /// Convenience aggregate that invokes each individual getter once. Total cost is
    /// order-of-magnitude 100 ms. The snapshot is not atomic across categories:
    /// <see cref="SystemSnapshot.CapturedAt"/> is the UTC start time.
    /// </remarks>
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
