namespace DeepSigma.SystemDiagnostics.Models;

public sealed record NetworkAdapterInfo(
    string Id,
    string Name,
    string Description,
    string MacAddress,
    string Type,
    bool IsUp,
    long SpeedBitsPerSecond,
    IReadOnlyList<string> IPv4Addresses,
    IReadOnlyList<string> IPv6Addresses);
