namespace DeepSigma.SystemDiagnostics.Enums;

/// <summary>
/// High-level classification of a storage volume.
/// </summary>
public enum DriveKind
{
    /// <summary>The drive type could not be determined.</summary>
    Unknown = 0,

    /// <summary>Fixed internal disk (HDD, SSD, NVMe).</summary>
    Fixed = 1,

    /// <summary>Removable media such as a USB stick or external drive.</summary>
    Removable = 2,

    /// <summary>Network-mapped volume (SMB, NFS, etc.).</summary>
    Network = 3,

    /// <summary>RAM-backed volume (e.g. tmpfs).</summary>
    Ram = 4,

    /// <summary>Optical disc drive (CD, DVD, Blu-ray).</summary>
    CdRom = 5,
}
