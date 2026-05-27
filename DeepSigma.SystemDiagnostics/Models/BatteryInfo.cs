using DeepSigma.SystemDiagnostics.Enums;

namespace DeepSigma.SystemDiagnostics.Models;

/// <summary>
/// Current state of a single battery. Most laptops expose one; desktops typically expose none.
/// </summary>
/// <param name="Name">Battery identifier — manufacturer/model string on Windows
/// (e.g. <c>"DELL 70N2F95"</c>) or sysfs node name on Linux (e.g. <c>"BAT0"</c>).</param>
/// <param name="ChargePercent">Remaining charge as an integer percent in <c>[0, 100]</c>,
/// or <c>null</c> if not reported.</param>
/// <param name="Status">Current power state.</param>
/// <param name="IsOnAcPower">True when external power is connected.
/// On Linux this is derived from <c>/sys/class/power_supply/.../online</c>; on Windows
/// it is inferred from the battery status code.</param>
public sealed record BatteryInfo(
    string Name,
    int? ChargePercent,
    BatteryStatus Status,
    bool IsOnAcPower);
