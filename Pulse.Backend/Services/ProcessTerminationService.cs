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
            "msedge",
            "electron",
            "pulse"
        };

        public bool TryCloseProcess(Process process, Process? foreground)
        {
            try
            {
                var name = process.ProcessName.ToLower();

                // ----------------------
                // SAFETY CHECKS
                // ----------------------
                if (process.SessionId == 0)
                    return false;

                if (_protectedApps.Contains(name))
                {
                    Console.WriteLine($"[Pulse] Protected app skipped: {process.ProcessName}");
                    return false;
                }

                if (process.Id == Process.GetCurrentProcess().Id)
                    return false;

                if (foreground != null && process.Id == foreground.Id)
                {
                    Console.WriteLine($"[Pulse] Skipping foreground app: {process.ProcessName}");
                    return false;
                }

                // ----------------------
                // SAFE CLOSE
                // ----------------------
                if (process.CloseMainWindow())
                {
                    Console.WriteLine($"[Pulse] Requested close: {process.ProcessName}");
                    return true;
                }

                // ----------------------
                // FORCE KILL (fallback)
                // ----------------------
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