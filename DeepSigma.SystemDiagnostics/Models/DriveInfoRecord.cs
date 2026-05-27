using DeepSigma.SystemDiagnostics.Enums;

namespace DeepSigma.SystemDiagnostics.Models;

/// <summary>
/// Snapshot of a storage volume's capacity and metadata.
/// </summary>
/// <param name="Name">Mount path or drive letter — e.g. <c>"C:\"</c> on Windows, <c>"/"</c> or <c>"/mnt/data"</c> on Linux.</param>
/// <param name="VolumeLabel">User-assigned volume label, or <c>null</c> when not set / unavailable.</param>
/// <param name="FileSystem">File-system format string — e.g. <c>"NTFS"</c>, <c>"ext4"</c>, <c>"btrfs"</c>. <c>null</c> when the drive is not ready.</param>
/// <param name="Kind">High-level drive classification.</param>
/// <param name="TotalBytes">Total capacity in bytes. <c>0</c> when the drive is not ready.</param>
/// <param name="AvailableBytes">Free space in bytes from the perspective of the current user. <c>0</c> when the drive is not ready.</param>
/// <param name="IsReady">True when the drive is mounted and its capacity could be read.</param>
public sealed record DriveInfoRecord(
    string Name,
    string? VolumeLabel,
    string? FileSystem,
    DriveKind Kind,
    ulong TotalBytes,
    ulong AvailableBytes,
    bool IsReady);
