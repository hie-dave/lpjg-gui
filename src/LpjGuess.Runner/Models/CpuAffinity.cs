using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LpjGuess.Runner.Models;

/// <summary>
/// This class is used to set the CPU affinity of a process.
/// </summary>
public class CpuAffinity : IDisposable
{
    /// <summary>
    /// The number of processors on the system.
    /// </summary>
    private static readonly int processorCount = Environment.ProcessorCount;

    /// <summary>
    /// The set of CPUs that have been assigned to processes.
    /// </summary>
    private static readonly HashSet<int> assignedCpus = new HashSet<int>();

    /// <summary>
    /// The CPU that this instance is assigned to.
    /// </summary>
    private readonly int cpu;

    /// <summary>
    /// Create a new <see cref="CpuAffinity"/> instance.
    /// </summary>
    /// <param name="cpu">The CPU to assign to.</param>
    private CpuAffinity(int cpu)
    {
        this.cpu = cpu;
    }

    /// <summary>
    /// Set the CPU affinity of the given process.
    /// </summary>
    /// <param name="process">The process to set the CPU affinity for.</param>
    public void SetAffinity(Process process)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            SetProcessorAffinity(process);
        }
    }

    /// <summary>
    /// Set the CPU affinity of the given process.
    /// </summary>
    /// <param name="proc">The process to set the CPU affinity for.</param>
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

    /// <summary>
    /// Acquire a CPU affinity for a process.
    /// </summary>
    /// <returns>A <see cref="CpuAffinity"/> instance.</returns>
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

    /// <inheritdoc />
    public void Dispose()
    {
        lock (assignedCpus)
        {
            assignedCpus.Remove(cpu);
        }
        GC.SuppressFinalize(this);
    }
}
