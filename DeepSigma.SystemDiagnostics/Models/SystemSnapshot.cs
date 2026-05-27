namespace DeepSigma.SystemDiagnostics.Models;

public sealed record SystemSnapshot(
    DateTimeOffset CapturedAt,
    OperatingSystemInfo OperatingSystem,
    CpuInfo Cpu,
    MemoryInfo Memory,
    IReadOnlyList<DriveInfoRecord> Drives,
    IReadOnlyList<NetworkAdapterInfo> NetworkAdapters,
    IReadOnlyList<GpuInfo> Gpus);
