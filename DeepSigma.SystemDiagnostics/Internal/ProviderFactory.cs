using System.Runtime.InteropServices;
using DeepSigma.SystemDiagnostics.Internal.Linux;
using DeepSigma.SystemDiagnostics.Internal.Windows;

namespace DeepSigma.SystemDiagnostics.Internal;

internal static class ProviderFactory
{
    public static ISystemInfoProvider Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsSystemInfoProvider();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxSystemInfoProvider();
        return new FallbackSystemInfoProvider();
    }
}
