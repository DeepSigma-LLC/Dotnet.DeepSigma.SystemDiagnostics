namespace DeepSigma.SystemDiagnostics.Enums;

/// <summary>
/// Current power state of a battery.
/// </summary>
public enum BatteryStatus
{
    /// <summary>Status could not be read or is not reported by the OS.</summary>
    Unknown = 0,

    /// <summary>Battery is currently charging from external power.</summary>
    Charging = 1,

    /// <summary>Battery is discharging — external power is not connected.</summary>
    Discharging = 2,

    /// <summary>Battery is fully charged (and typically still on external power).</summary>
    Full = 3,

    /// <summary>External power is connected but the battery is not accepting charge
    /// (e.g. battery saver or manufacturer charge-limit thresholds).</summary>
    NotCharging = 4,
}
