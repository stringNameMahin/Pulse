using System.Diagnostics;

namespace Pulse.Backend.Services
{
    public class MonitoringService
    {
        private const long MemoryThresholdMb = 500;

        public bool IsHeavyLoad()
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    double memoryMb = process.WorkingSet64 / (1024.0 * 1024.0);

                    if (memoryMb >= MemoryThresholdMb)
                        return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Pulse Warning] Monitoring failed: {ex.Message}");
                }
            }

            return false;
        }

        public List<Process> GetHeavyProcesses()
        {
            var heavy = new List<Process>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    double memoryMb = process.WorkingSet64 / (1024.0 * 1024.0);

                    if (memoryMb >= MemoryThresholdMb)
                        heavy.Add(process);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Pulse Warning] Monitoring failed: {ex.Message}");
                }
            }

            return heavy;
        }
    }
}