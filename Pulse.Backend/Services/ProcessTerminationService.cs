using System;
using System.Diagnostics;

namespace Pulse.Backend.Services
{
    public class ProcessTerminationService
    {
        private readonly HashSet<string> _protectedApps = new()
        {
            "explorer",
            "system",
            "idle",
            "winlogon",
            "csrss",
            "memory compression",

            "devenv",
            "code",
            //"chrome",
            //"opera",
            "msedge",
            "electron"
        };

        public List<Process> GetHeavyProcesses(double thresholdMb = 500)
        {
            var heavy = new List<Process>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    double memoryMb = process.WorkingSet64 / (1024.0 * 1024.0);

                    if (memoryMb >= thresholdMb)
                        heavy.Add(process);
                }
                catch { }
            }

            return heavy;
        }

        public bool TryCloseProcess(Process process, Process? foreground)
        {
            try
            {
                var name = process.ProcessName.ToLower();

                if (process.SessionId == 0)
                    return false;

                if (_protectedApps.Contains(name))
                {
                    Console.WriteLine($"[Pulse] Protected app skipped: {process.ProcessName}");
                    return false;
                }

                if (process.Id == Process.GetCurrentProcess().Id)
                    return false;

                if (name.Contains("pulse") || name.Contains("electron"))
                    return false;

                if (foreground != null && process.Id == foreground.Id)
                {
                    Console.WriteLine($"[Pulse] Skipping foreground app: {process.ProcessName}");
                    return false;
                }

                if (process.CloseMainWindow())
                {
                    Console.WriteLine($"[Pulse] Requested close: {process.ProcessName}");
                    return true;
                }

                process.Kill();
                Console.WriteLine($"[Pulse] Force killed: {process.ProcessName}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Pulse Warning] Failed to terminate {process.ProcessName}: {ex.Message}");
                return false;
            }
        }
    }
}