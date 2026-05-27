using DeepSigma.SystemDiagnostics.Enums;

namespace DeepSigma.SystemDiagnostics.Models;

public sealed record OperatingSystemInfo(
    OSFamily Family,
    string Description,
    string Version,
    string MachineName,
    string UserName,
    bool Is64Bit);
