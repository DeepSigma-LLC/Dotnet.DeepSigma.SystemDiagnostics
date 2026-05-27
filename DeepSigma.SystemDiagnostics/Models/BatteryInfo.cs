using DeepSigma.SystemDiagnostics.Enums;

namespace DeepSigma.SystemDiagnostics.Models;

public sealed record BatteryInfo(
    string Name,
    int? ChargePercent,
    BatteryStatus Status,
    bool IsOnAcPower);
