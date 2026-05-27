using System.Management;
using System.Runtime.Versioning;

namespace DeepSigma.SystemDiagnostics.Internal.Windows;

[SupportedOSPlatform("windows")]
internal static class WmiHelper
{
    public static IEnumerable<ManagementObject> Query(string wql)
    {
        ManagementObjectSearcher searcher;
        try
        {
            searcher = new ManagementObjectSearcher(wql);
        }
        catch
        {
            yield break;
        }

        using (searcher)
        {
            ManagementObjectCollection collection;
            try
            {
                collection = searcher.Get();
            }
            catch
            {
                yield break;
            }

            using (collection)
            {
                foreach (var obj in collection)
                {
                    yield return (ManagementObject)obj;
                }
            }
        }
    }

    public static T? Read<T>(ManagementBaseObject obj, string property) where T : struct
    {
        try
        {
            var raw = obj[property];
            if (raw is null) return null;
            return (T)Convert.ChangeType(raw, typeof(T));
        }
        catch
        {
            return null;
        }
    }

    public static string? ReadString(ManagementBaseObject obj, string property)
    {
        try
        {
            return obj[property]?.ToString();
        }
        catch
        {
            return null;
        }
    }
}
