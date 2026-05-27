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
        if (SystemDiagnostics.CurrentOS == OSFamily.Unknown) return;

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

        Assert.Equal(SystemDiagnostics.GetOperatingSystem(), snapshot.OperatingSystem);
        Assert.Equal(SystemDiagnostics.GetCpu(), snapshot.Cpu);
        Assert.True(snapshot.CapturedAt <= DateTimeOffset.UtcNow);
        Assert.True(snapshot.CapturedAt > DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void OS_And_Cpu_Are_Cached_Same_Reference()
    {
        Assert.Same(SystemDiagnostics.GetOperatingSystem(), SystemDiagnostics.GetOperatingSystem());
        Assert.Same(SystemDiagnostics.GetCpu(), SystemDiagnostics.GetCpu());
    }
}
