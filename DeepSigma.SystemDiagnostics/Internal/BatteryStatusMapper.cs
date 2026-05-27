using DeepSigma.SystemDiagnostics.Enums;

namespace DeepSigma.SystemDiagnostics.Internal;

internal static class BatteryStatusMapper
{
    // Win32_Battery.BatteryStatus codes per the WMI docs.
    // Returns the mapped status and whether AC power is connected.
    public static (BatteryStatus Status, bool IsOnAcPower) FromWindowsCode(ushort? code) => code switch
    {
        1 => (BatteryStatus.Discharging, false),
        2 => (BatteryStatus.Full, true),
        3 => (BatteryStatus.Full, true),
        4 => (BatteryStatus.Discharging, false),
        5 => (BatteryStatus.Discharging, false),
        6 => (BatteryStatus.Charging, true),
        7 => (BatteryStatus.Charging, true),
        8 => (BatteryStatus.Charging, true),
        9 => (BatteryStatus.Charging, true),
        11 => (BatteryStatus.Charging, true),
        _ => (BatteryStatus.Unknown, false),
    };

    public static BatteryStatus FromLinuxStatusFile(string? raw) => raw switch
    {
        "Charging" => BatteryStatus.Charging,
        "Discharging" => BatteryStatus.Discharging,
        "Full" => BatteryStatus.Full,
        "Not charging" => BatteryStatus.NotCharging,
        _ => BatteryStatus.Unknown,
    };
}
