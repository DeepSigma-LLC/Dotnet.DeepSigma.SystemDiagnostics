using System.Text.RegularExpressions;
using DeepSigma.SystemDiagnostics.Models;

namespace DeepSigma.SystemDiagnostics.Internal;

internal static class NetworkAdapterFilter
{
    private static readonly string[] NoiseKeywords =
    [
        "Filter",
        "Scheduler",
        "WFP",
        "Kernel Debugger",
        "Miniport",
    ];

    private static readonly Regex NdisSubInstanceSuffix =
        new(@"-\d{4}$", RegexOptions.Compiled);

    public static bool IsCurated(NetworkAdapterInfo adapter)
    {
        if (string.Equals(adapter.Type, "Tunnel", StringComparison.OrdinalIgnoreCase))
            return false;

        if (NdisSubInstanceSuffix.IsMatch(adapter.Name))
            return false;

        foreach (var keyword in NoiseKeywords)
        {
            if (adapter.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }
}
