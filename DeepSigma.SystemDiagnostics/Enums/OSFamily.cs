namespace DeepSigma.SystemDiagnostics.Enums;

/// <summary>
/// Operating-system family of the host machine.
/// </summary>
public enum OSFamily
{
    /// <summary>The platform could not be identified (e.g. macOS, FreeBSD).</summary>
    Unknown = 0,

    /// <summary>Microsoft Windows.</summary>
    Windows = 1,

    /// <summary>Any Linux distribution.</summary>
    Linux = 2,
}
