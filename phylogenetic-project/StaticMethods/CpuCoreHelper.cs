using System;
using System.Runtime.InteropServices;

namespace phylogenetic_project.StaticMethods;

public static class CpuCoreHelper
{
    [DllImport("kernel32.dll", SetLastError = false)]
    private static extern uint GetCurrentProcessorNumber();

    [DllImport("libc")]
    private static extern int sched_getcpu();

    public static int? GetCurrentCore()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return (int)GetCurrentProcessorNumber();
            else
                return sched_getcpu();
        }
        catch
        {
            return null;
        }
    }
}
