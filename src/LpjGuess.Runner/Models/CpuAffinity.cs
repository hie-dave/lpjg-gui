using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LpjGuess.Runner.Models;

public class CpuAffinity : IDisposable
{
    private static readonly int processorCount = Environment.ProcessorCount;
    private static readonly HashSet<int> assignedCpus = new HashSet<int>();

    private readonly int cpu;

    private CpuAffinity(int cpu)
    {
        this.cpu = cpu;
    }

    public void SetAffinity(Process process)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            SetProcessorAffinity(process);
        }
    }

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    private void SetProcessorAffinity(Process proc)
    {
        try
        {
            // Create a processor mask with only one bit set (for the assigned core)
            IntPtr affinityMask = new IntPtr(1L << cpu);
            
            // Set the process affinity
            proc.ProcessorAffinity = affinityMask;
        }
        catch (Exception ex)
        {
            // Log but don't fail if setting affinity fails
            Console.Error.WriteLine($"Warning: Failed to set processor affinity: {ex.Message}");
        }
    }

    public static CpuAffinity Acquire()
    {
        lock (assignedCpus)
        {
            int i = Enumerable.Range(0, processorCount)
                              .FirstOrDefault(i => !assignedCpus.Contains(i));
            assignedCpus.Add(i);
            return new CpuAffinity(i);
        }
    }

    public void Dispose()
    {
        lock (assignedCpus)
        {
            assignedCpus.Remove(cpu);
        }
    }
}
