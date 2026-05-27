using DeepSigma.SystemDiagnostics.Enums;
using DeepSigma.SystemDiagnostics.Internal;
using Xunit;

namespace DeepSigma.SystemDiagnostics.Test;

public class BatteryStatusMapperTests
{
    [Theory]
    [InlineData(1, BatteryStatus.Discharging, false)]
    [InlineData(2, BatteryStatus.Full, true)]
    [InlineData(3, BatteryStatus.Full, true)]
    [InlineData(4, BatteryStatus.Discharging, false)]
    [InlineData(5, BatteryStatus.Discharging, false)]
    [InlineData(6, BatteryStatus.Charging, true)]
    [InlineData(7, BatteryStatus.Charging, true)]
    [InlineData(8, BatteryStatus.Charging, true)]
    [InlineData(9, BatteryStatus.Charging, true)]
    [InlineData(11, BatteryStatus.Charging, true)]
    public void Windows_Documented_Codes_Map_Correctly(ushort code, BatteryStatus expected, bool expectedOnAc)
    {
        var (status, onAc) = BatteryStatusMapper.FromWindowsCode(code);
        Assert.Equal(expected, status);
        Assert.Equal(expectedOnAc, onAc);
    }

    [Fact]
    public void Windows_Null_Code_Maps_To_Unknown()
    {
        var (status, onAc) = BatteryStatusMapper.FromWindowsCode(null);
        Assert.Equal(BatteryStatus.Unknown, status);
        Assert.False(onAc);
    }

    [Fact]
    public void Windows_Code_10_Maps_To_Unknown()
    {
        var (status, onAc) = BatteryStatusMapper.FromWindowsCode(10);
        Assert.Equal(BatteryStatus.Unknown, status);
        Assert.False(onAc);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(0)]
    [InlineData(ushort.MaxValue)]
    public void Windows_Out_Of_Range_Codes_Map_To_Unknown(ushort code)
    {
        var (status, onAc) = BatteryStatusMapper.FromWindowsCode(code);
        Assert.Equal(BatteryStatus.Unknown, status);
        Assert.False(onAc);
    }

    [Theory]
    [InlineData("Charging", BatteryStatus.Charging)]
    [InlineData("Discharging", BatteryStatus.Discharging)]
    [InlineData("Full", BatteryStatus.Full)]
    [InlineData("Not charging", BatteryStatus.NotCharging)]
    public void Linux_Documented_Strings_Map_Correctly(string raw, BatteryStatus expected)
    {
        Assert.Equal(expected, BatteryStatusMapper.FromLinuxStatusFile(raw));
    }

    [Fact]
    public void Linux_Match_Is_Case_Sensitive()
    {
        // The Linux kernel writes these exact strings; case-insensitive matching could mask
        // a kernel-side change we'd want to know about.
        Assert.Equal(BatteryStatus.Unknown, BatteryStatusMapper.FromLinuxStatusFile("charging"));
        Assert.Equal(BatteryStatus.Unknown, BatteryStatusMapper.FromLinuxStatusFile("FULL"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Unknown")]
    [InlineData("Some new state")]
    public void Linux_Unknown_Strings_Map_To_Unknown(string? raw)
    {
        Assert.Equal(BatteryStatus.Unknown, BatteryStatusMapper.FromLinuxStatusFile(raw));
    }
}
