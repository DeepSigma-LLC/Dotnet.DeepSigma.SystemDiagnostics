using DeepSigma.SystemDiagnostics.Models;

namespace DeepSigma.SystemDiagnostics.Internal;

internal interface ISystemInfoProvider
{
    OperatingSystemInfo GetOperatingSystem();
    CpuInfo GetCpu();
    MemoryInfo GetMemory();
    IReadOnlyList<DriveInfoRecord> GetDrives();
    IReadOnlyList<NetworkAdapterInfo> GetNetworkAdapters();
    IReadOnlyList<GpuInfo> GetGpus();
    IReadOnlyList<BatteryInfo> GetBatteries();
    IReadOnlyList<TemperatureReading> GetTemperatures();
}
