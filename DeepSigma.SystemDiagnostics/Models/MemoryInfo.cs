namespace DeepSigma.SystemDiagnostics.Models;

/// <summary>
/// Physical-memory totals at the moment of capture.
/// </summary>
/// <param name="TotalBytes">Total physical RAM in bytes. <c>0</c> indicates the value could not be read.</param>
/// <param name="AvailableBytes">Bytes of physical memory available for new allocations, or
/// <c>null</c> if the OS did not report it or parsing failed.</param>
public sealed record MemoryInfo(
    ulong TotalBytes,
    ulong? AvailableBytes);
