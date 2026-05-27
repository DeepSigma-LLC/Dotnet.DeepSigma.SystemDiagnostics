using DeepSigma.SystemDiagnostics.Enums;
using DeepSigma.SystemDiagnostics.Internal;
using DeepSigma.SystemDiagnostics.Models;

namespace DeepSigma.SystemDiagnostics;

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
        _provider.Value.GetNetworkAdapters();

    public static IReadOnlyList<GpuInfo> GetGpus() => _gpus.Value;

    public static SystemSnapshot GetSnapshot() => new(
        CapturedAt: DateTimeOffset.UtcNow,
        OperatingSystem: GetOperatingSystem(),
        Cpu: GetCpu(),
        Memory: GetMemory(),
        Drives: GetDrives(),
        NetworkAdapters: GetNetworkAdapters(),
        Gpus: GetGpus());
}
