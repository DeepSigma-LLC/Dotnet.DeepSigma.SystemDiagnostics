namespace DeepSigma.SystemDiagnostics.Models;

public sealed record MemoryInfo(
    ulong TotalBytes,
    ulong? AvailableBytes);
