namespace Pulse.Backend.Services
{
    using System.Diagnostics;

    public class MonitoringService
    {
        private const long memoryThresholdMb = 500;

        public bool IsHeavyLoad()
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    double memoryMb = process.WorkingSet64 / (1024.0 * 1024.0);

                    if (memoryMb >= memoryThresholdMb)
                        return true;
                }
                catch { }
            }

            return false;
        }
        public List<Process> GetHeavyProcesses()
        {
            const long memoryThresholdMb = 500;
            var heavy = new List<Process>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    double memoryMb = process.WorkingSet64 / (1024.0 * 1024.0);

                    if (memoryMb >= memoryThresholdMb)
                        heavy.Add(process);
                }
                catch { }
            }

            return heavy;
        }
    }
}
