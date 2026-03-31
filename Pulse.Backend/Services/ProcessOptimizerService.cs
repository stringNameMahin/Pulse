
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pulse.Backend.Services
{
    public class ProcessOptimizerService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public Process? GetForegroundProcess()
        {
            try
            {
                var handle = GetForegroundWindow();

                if (handle == IntPtr.Zero)
                    return null;

                GetWindowThreadProcessId(handle, out uint pid);

                return Process.GetProcessById((int)pid);
            }
            catch
            {
                return null;
            }
        }

        public void ApplyPriority(Process process, string profileId)
        {
            try
            {
                var name = process.ProcessName.ToLower();

                if (process.Id == 0 || name.Contains("system"))
                    return;

                if (name.Contains("electron") || name.Contains("pulse"))
                    return;

                var desiredPriority = profileId == "high"
                    ? ProcessPriorityClass.AboveNormal
                    : ProcessPriorityClass.Normal;

                if (process.PriorityClass == desiredPriority)
                    return;

                process.PriorityClass = desiredPriority;

                Console.WriteLine($"[Pulse] Optimized foreground process: {process.ProcessName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Pulse Warning] Failed to optimize {process.ProcessName}: {ex.Message}");
            }
        }
    }
}