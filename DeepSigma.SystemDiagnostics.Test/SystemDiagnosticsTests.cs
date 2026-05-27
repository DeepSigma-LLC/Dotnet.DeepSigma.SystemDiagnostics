using System.Runtime.InteropServices;
using DeepSigma.SystemDiagnostics;
using DeepSigma.SystemDiagnostics.Enums;
using Xunit;

namespace DeepSigma.SystemDiagnostics.Test;

public class SystemDiagnosticsTests
{
    [Fact]
    public void OS_Family_Matches_Runtime()
    {
        var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSFamily.Windows
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSFamily.Linux
            : OSFamily.Unknown;
        Assert.Equal(expected, SystemDiagnostics.CurrentOS);
        Assert.Equal(expected, SystemDiagnostics.GetOperatingSystem().Family);
    }

    [Fact]
    public void OperatingSystem_Has_Basic_Identity()
    {
        var os = SystemDiagnostics.GetOperatingSystem();
        Assert.False(string.IsNullOrWhiteSpace(os.MachineName));
        Assert.False(string.IsNullOrWhiteSpace(os.Description));
    }

    [Fact]
    public void Cpu_LogicalCores_Equals_ProcessorCount()
    {
        var cpu = SystemDiagnostics.GetCpu();
        Assert.Equal(Environment.ProcessorCount, cpu.LogicalCores);
        Assert.True(cpu.LogicalCores >= 1);
    }

    [Fact]
    public void Cpu_Name_Not_Empty()
    {
        var cpu = SystemDiagnostics.GetCpu();
        Assert.False(string.IsNullOrWhiteSpace(cpu.Name));
    }

    [Fact]
    public void Memory_TotalBytes_Positive_When_Supported()
    {
        if (SystemDiagnostics.CurrentOS == OSFamily.Unknown)
            Assert.Skip("Memory reporting requires Windows or Linux");

        var memory = SystemDiagnostics.GetMemory();
        Assert.True(memory.TotalBytes > 0, "Expected positive total memory on supported OS");
        if (memory.AvailableBytes.HasValue)
            Assert.True(memory.AvailableBytes.Value <= memory.TotalBytes);
    }

    [Fact]
    public void Drives_Has_At_Least_One_Ready_Drive()
    {
        var drives = SystemDiagnostics.GetDrives();
        Assert.NotEmpty(drives);
        Assert.Contains(drives, d => d.IsReady);

        if (SystemDiagnostics.CurrentOS == OSFamily.Windows)
        {
            Assert.Contains(drives, d => d.IsReady && d.Name.Length >= 2 && d.Name[1] == ':');
        }
        else if (SystemDiagnostics.CurrentOS == OSFamily.Linux)
        {
            Assert.Contains(drives, d => d.IsReady && d.Name == "/");
        }
    }

    [Fact]
    public void Network_Has_Loopback()
    {
        var nics = SystemDiagnostics.GetNetworkAdapters();
        Assert.NotEmpty(nics);
        Assert.Contains(nics, n => n.Type.Contains("Loopback", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Gpus_Returns_Without_Throwing()
    {
        var gpus = SystemDiagnostics.GetGpus();
        Assert.NotNull(gpus);
    }

    [Fact]
    public void Snapshot_Matches_Individual_Getters()
    {
        var snapshot = SystemDiagnostics.GetSnapshot();

        // Cached fields: exact equality (same Lazy<T> reference)
        Assert.Equal(SystemDiagnostics.GetOperatingSystem(), snapshot.OperatingSystem);
        Assert.Equal(SystemDiagnostics.GetCpu(), snapshot.Cpu);
        Assert.Same(SystemDiagnostics.GetGpus(), snapshot.Gpus);

        // Live fields: re-querying changes ephemeral values but list shape should match
        Assert.NotNull(snapshot.Drives);
        Assert.NotNull(snapshot.NetworkAdapters);
        Assert.NotNull(snapshot.Temperatures);
        Assert.NotNull(snapshot.Memory);
        Assert.Equal(SystemDiagnostics.GetDrives().Count, snapshot.Drives.Count);
        Assert.Equal(SystemDiagnostics.GetNetworkAdapters().Count, snapshot.NetworkAdapters.Count);

        // Timestamp sanity
        Assert.True(snapshot.CapturedAt <= DateTimeOffset.UtcNow);
        Assert.True(snapshot.CapturedAt > DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void OS_And_Cpu_Are_Cached_Same_Reference()
    {
        Assert.Same(SystemDiagnostics.GetOperatingSystem(), SystemDiagnostics.GetOperatingSystem());
        Assert.Same(SystemDiagnostics.GetCpu(), SystemDiagnostics.GetCpu());
    }

    [Fact]
    public void Curated_NetworkAdapters_Is_Subset_Of_All()
    {
        var curated = SystemDiagnostics.GetNetworkAdapters();
        var all = SystemDiagnostics.GetAllNetworkAdapters();
        Assert.True(curated.Count <= all.Count);

        var allIds = all.Select(a => a.Id).ToHashSet();
        foreach (var adapter in curated)
            Assert.Contains(adapter.Id, allIds);
    }

    [Fact]
    public void Curated_NetworkAdapters_Excludes_Tunnels()
    {
        var curated = SystemDiagnostics.GetNetworkAdapters();
        Assert.DoesNotContain(curated, a =>
            string.Equals(a.Type, "Tunnel", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Curated_NetworkAdapters_Excludes_Filter_Drivers()
    {
        var noiseKeywords = new[] { "Filter", "Scheduler", "WFP", "Kernel Debugger", "Miniport" };
        var curated = SystemDiagnostics.GetNetworkAdapters();
        foreach (var adapter in curated)
        {
            foreach (var keyword in noiseKeywords)
            {
                Assert.False(
                    adapter.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase),
                    $"Curated adapter '{adapter.Name}' contains noise keyword '{keyword}' in description: {adapter.Description}");
            }
        }
    }

    [Fact]
    public void All_NetworkAdapters_Includes_Loopback()
    {
        var all = SystemDiagnostics.GetAllNetworkAdapters();
        Assert.Contains(all, n => n.Type.Contains("Loopback", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Curated_NetworkAdapters_Keeps_Loopback()
    {
        var curated = SystemDiagnostics.GetNetworkAdapters();
        Assert.Contains(curated, n => n.Type.Contains("Loopback", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Batteries_Returns_Without_Throwing()
    {
        var batteries = SystemDiagnostics.GetBatteries();
        Assert.NotNull(batteries);
    }

    [Fact]
    public void Battery_ChargePercent_Within_Range_When_Present()
    {
        foreach (var b in SystemDiagnostics.GetBatteries())
        {
            if (b.ChargePercent.HasValue)
            {
                Assert.InRange(b.ChargePercent.Value, 0, 100);
            }
        }
    }

    [Fact]
    public void Battery_Status_Is_Defined_Enum()
    {
        foreach (var b in SystemDiagnostics.GetBatteries())
        {
            Assert.True(Enum.IsDefined(b.Status));
        }
    }

    [Fact]
    public void Temperatures_Returns_Without_Throwing()
    {
        var temps = SystemDiagnostics.GetTemperatures();
        Assert.NotNull(temps);
    }

    [Fact]
    public void Temperatures_Empty_On_Windows()
    {
        if (SystemDiagnostics.CurrentOS != OSFamily.Windows)
            Assert.Skip("Test only applies to Windows");
        Assert.Empty(SystemDiagnostics.GetTemperatures());
    }

    [Fact]
    public void Temperature_Readings_Are_In_Plausible_Range()
    {
        foreach (var t in SystemDiagnostics.GetTemperatures())
        {
            Assert.InRange(t.Celsius, -50.0, 200.0);
            Assert.False(string.IsNullOrWhiteSpace(t.ChipName));
        }
    }

    [Fact]
    public void Temperature_Thresholds_Are_Ordered_When_Both_Present()
    {
        foreach (var t in SystemDiagnostics.GetTemperatures())
        {
            if (t.MaxCelsius.HasValue && t.CritCelsius.HasValue)
                Assert.True(t.MaxCelsius.Value <= t.CritCelsius.Value);
        }
    }

    [Fact]
    public void Snapshot_Includes_Temperatures()
    {
        var snapshot = SystemDiagnostics.GetSnapshot();
        Assert.NotNull(snapshot.Temperatures);
    }
}
