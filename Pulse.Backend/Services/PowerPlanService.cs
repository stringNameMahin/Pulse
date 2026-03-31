using System.Diagnostics;
using System.Security.Principal;

namespace Pulse.Backend.Services
{
    public class PowerPlanService
    {
        public bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public bool SetPowerPlan(string profileId)
        {
            string guid = profileId switch
            {
                "balanced" => "381b4222-f694-41f0-9685-ff5bb260df2e",
                "high" => "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                _ => throw new ArgumentException("Invalid profile")
            };

            return RunPowerCfg(guid);
        }

        private bool RunPowerCfg(string guid)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = $"/setactive {guid}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}