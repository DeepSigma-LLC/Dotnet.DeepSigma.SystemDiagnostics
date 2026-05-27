using DeepSigma.SystemDiagnostics.Enums;

namespace DeepSigma.SystemDiagnostics.Models;

public sealed record DriveInfoRecord(
    string Name,
    string? VolumeLabel,
    string? FileSystem,
    DriveKind Kind,
    ulong TotalBytes,
    ulong AvailableBytes,
    bool IsReady);
