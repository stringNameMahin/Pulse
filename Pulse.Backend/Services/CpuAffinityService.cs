using System.Diagnostics;

namespace Pulse.Backend.Services
{
    public class CpuAffinityService
    {
        public void ApplyAffinity(Process process, string profileId)
        {
            try
            {
                var name = process.ProcessName.ToLower();

                if (process.Id == 0 || name.Contains("system"))
                    return;

                if (name.Contains("electron") || name.Contains("pulse"))
                    return;

                if (process.HasExited)
                    return;

                if (process.MainWindowHandle == IntPtr.Zero)
                    return;

                int coreCount = Environment.ProcessorCount;
                int coresToUse = coreCount;

                if (profileId == "balanced")
                {
                    coresToUse = (int)(coreCount * 0.75);
                }
                else if (profileId == "quiet")
                {
                    coresToUse = coreCount / 2;
                }

                coresToUse = Math.Max(2, coresToUse);
                coresToUse = Math.Min(coreCount, coresToUse);

                long mask = 0;

                for (int i = 0; i < coresToUse; i++)
                {
                    mask |= (1L << i);
                }

                IntPtr desiredMask = (IntPtr)mask;

                if (process.ProcessorAffinity == desiredMask)
                    return;

                process.ProcessorAffinity = desiredMask;

                Console.WriteLine($"[Pulse] Affinity → {process.ProcessName} ({coresToUse}/{coreCount} cores)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Pulse Warning] Affinity failed for {process.ProcessName}: {ex.Message}");
            }
        }
    }
}