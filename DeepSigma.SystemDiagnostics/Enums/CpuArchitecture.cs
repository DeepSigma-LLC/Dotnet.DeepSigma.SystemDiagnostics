namespace DeepSigma.SystemDiagnostics.Enums;

/// <summary>
/// CPU instruction-set architecture of the host machine.
/// </summary>
public enum CpuArchitecture
{
    /// <summary>The architecture could not be identified.</summary>
    Unknown = 0,

    /// <summary>32-bit x86 (i386, i686).</summary>
    X86 = 1,

    /// <summary>64-bit x86 (AMD64 / Intel 64).</summary>
    X64 = 2,

    /// <summary>32-bit ARM.</summary>
    Arm = 3,

    /// <summary>64-bit ARM (AArch64).</summary>
    Arm64 = 4,
}
