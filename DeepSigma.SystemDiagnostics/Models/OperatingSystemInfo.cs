using DeepSigma.SystemDiagnostics.Enums;

namespace DeepSigma.SystemDiagnostics.Models;

/// <summary>
/// Identity and version information about the host operating system.
/// </summary>
/// <param name="Family">High-level OS family for switch/pivot logic.</param>
/// <param name="Description">
/// Free-form OS description as reported by <c>RuntimeInformation.OSDescription</c>
/// (e.g. <c>"Microsoft Windows 10.0.26100"</c> or <c>"Linux 6.6.32-generic #1 SMP"</c>).
/// </param>
/// <param name="Version">
/// Dotted version string. On Windows 11 hosts the major version is normalized to <c>11</c>
/// (the underlying NT version remains <c>10.0.x</c>; see <see cref="Description"/> for the
/// raw OS string).
/// </param>
/// <param name="MachineName">Hostname / machine name (<c>Environment.MachineName</c>).</param>
/// <param name="UserName">Username of the current process owner (<c>Environment.UserName</c>).</param>
/// <param name="Is64Bit">True when the OS is 64-bit, regardless of process bitness.</param>
public sealed record OperatingSystemInfo(
    OSFamily Family,
    string Description,
    string Version,
    string MachineName,
    string UserName,
    bool Is64Bit);
