namespace DeepSigma.SystemDiagnostics.Models;

/// <summary>
/// Description of a single network interface (NIC) at the moment of capture.
/// </summary>
/// <param name="Id">OS-assigned interface identifier (GUID on Windows, device name on Linux).</param>
/// <param name="Name">Human-readable adapter name — e.g. <c>"Wi-Fi"</c>, <c>"eth0"</c>, <c>"Ethernet"</c>.</param>
/// <param name="Description">Driver/hardware description (e.g. <c>"Intel(R) Wi-Fi 6 AX201"</c>).</param>
/// <param name="MacAddress">Hardware address formatted as <c>XX:XX:XX:XX:XX:XX</c>, or empty string when the interface has no MAC (loopback, tunnels).</param>
/// <param name="Type">Interface type from <c>NetworkInterfaceType</c> — e.g. <c>"Ethernet"</c>, <c>"Wireless80211"</c>, <c>"Loopback"</c>, <c>"Tunnel"</c>.</param>
/// <param name="IsUp">True when the operational status is <c>Up</c>.</param>
/// <param name="SpeedBitsPerSecond">Link speed in bits per second as reported by the OS, or <c>-1</c> if unknown. Values may be inflated for loopback / virtual adapters.</param>
/// <param name="IPv4Addresses">All IPv4 unicast addresses assigned to the interface.</param>
/// <param name="IPv6Addresses">All IPv6 unicast addresses (link-local included).</param>
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
