using DeepSigma.SystemDiagnostics.Internal;
using DeepSigma.SystemDiagnostics.Models;
using Xunit;

namespace DeepSigma.SystemDiagnostics.Test;

public class NetworkAdapterFilterTests
{
    private static NetworkAdapterInfo Adapter(
        string name = "Ethernet",
        string description = "Realtek PCIe GbE Family Controller",
        string type = "Ethernet") =>
        new(
            Id: "test-id",
            Name: name,
            Description: description,
            MacAddress: "00:00:00:00:00:00",
            Type: type,
            IsUp: true,
            SpeedBitsPerSecond: 1_000_000_000,
            IPv4Addresses: Array.Empty<string>(),
            IPv6Addresses: Array.Empty<string>());

    [Fact]
    public void Plain_Ethernet_Adapter_Is_Curated()
    {
        Assert.True(NetworkAdapterFilter.IsCurated(Adapter()));
    }

    [Fact]
    public void Loopback_Is_Curated()
    {
        Assert.True(NetworkAdapterFilter.IsCurated(Adapter(
            name: "Loopback Pseudo-Interface 1",
            description: "Software Loopback Interface 1",
            type: "Loopback")));
    }

    [Fact]
    public void Wifi_Is_Curated()
    {
        Assert.True(NetworkAdapterFilter.IsCurated(Adapter(
            name: "Wi-Fi",
            description: "Intel(R) Wi-Fi 6 AX201 160MHz",
            type: "Wireless80211")));
    }

    [Fact]
    public void Tunnel_Type_Is_Excluded()
    {
        Assert.False(NetworkAdapterFilter.IsCurated(Adapter(
            name: "Teredo Tunneling Pseudo-Interface",
            description: "Teredo Tunneling Pseudo-Interface",
            type: "Tunnel")));
    }

    [Fact]
    public void Tunnel_Type_Match_Is_Case_Insensitive()
    {
        Assert.False(NetworkAdapterFilter.IsCurated(Adapter(type: "tunnel")));
        Assert.False(NetworkAdapterFilter.IsCurated(Adapter(type: "TUNNEL")));
    }

    [Theory]
    [InlineData("Filter")]
    [InlineData("Scheduler")]
    [InlineData("WFP")]
    [InlineData("Kernel Debugger")]
    [InlineData("Miniport")]
    public void Description_Contains_Noise_Keyword_Is_Excluded(string keyword)
    {
        var adapter = Adapter(description: $"Some {keyword} driver instance");
        Assert.False(NetworkAdapterFilter.IsCurated(adapter));
    }

    [Fact]
    public void Description_Keyword_Match_Is_Case_Insensitive()
    {
        Assert.False(NetworkAdapterFilter.IsCurated(Adapter(description: "qos packet scheduler")));
        Assert.False(NetworkAdapterFilter.IsCurated(Adapter(description: "WFP NATIVE MAC LAYER")));
    }

    [Fact]
    public void Name_With_Ndis_Suffix_Is_Excluded()
    {
        Assert.False(NetworkAdapterFilter.IsCurated(Adapter(
            name: "Wi-Fi-WFP Native MAC Layer LightWeight Filter-0000",
            description: "WFP Native MAC Layer LightWeight Filter")));
    }

    [Fact]
    public void Name_With_Four_Digit_Suffix_Excluded_Even_Without_Noise_Description()
    {
        Assert.False(NetworkAdapterFilter.IsCurated(Adapter(
            name: "Adapter-1234",
            description: "Clean description here")));
    }

    [Fact]
    public void Three_Digit_Suffix_Is_Not_Excluded()
    {
        Assert.True(NetworkAdapterFilter.IsCurated(Adapter(
            name: "Adapter-123",
            description: "Clean description")));
    }

    [Fact]
    public void Five_Digit_Suffix_Is_Not_Excluded()
    {
        Assert.True(NetworkAdapterFilter.IsCurated(Adapter(
            name: "Adapter-12345",
            description: "Clean description")));
    }
}
