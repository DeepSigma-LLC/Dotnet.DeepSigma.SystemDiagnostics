namespace DeepSigma.SystemDiagnostics.Models;

/// <summary>
/// Description of a graphics adapter detected on the host.
/// </summary>
/// <param name="Name">Adapter name — e.g. <c>"NVIDIA GeForce RTX 3050 Laptop GPU"</c> on Windows
/// or a PCI ID like <c>"NVIDIA 2520"</c> on Linux.</param>
/// <param name="Vendor">Vendor string (e.g. <c>"NVIDIA"</c>, <c>"AMD"</c>, <c>"Intel Corporation"</c>),
/// or <c>null</c> when not available.</param>
/// <param name="DriverVersion">Driver version reported by the OS, or <c>null</c> if not exposed
/// (Linux <c>/sys/class/drm</c> does not surface a driver version).</param>
/// <param name="AdapterRamBytes">
/// VRAM in bytes, or <c>null</c> if unavailable or unreliable.
/// On Windows the source is <c>Win32_VideoController.AdapterRAM</c>, a uint32 field; values at
/// the 4 GiB cap are reported as <c>null</c> rather than misleadingly showing 4 GiB on
/// higher-VRAM cards.
/// </param>
public sealed record GpuInfo(
    string Name,
    string? Vendor,
    string? DriverVersion,
    ulong? AdapterRamBytes);
