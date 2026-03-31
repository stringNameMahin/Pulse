using Pulse.Backend.Services;
using System.Diagnostics;

namespace Pulse.Backend
{
    public class ProfileManager
    {
        private string _currentProfileId = "balanced";
        private readonly MonitoringService _monitoring;
        private readonly PowerPlanService _powerService;
        public ProfileManager(MonitoringService monitoring, PowerPlanService powerService)
        {
            _monitoring = monitoring;
            _powerService = powerService;
        }

        public List<PerformanceProfile> Profiles { get; } = new()
    {
        new("balanced", "Balanced"),
        new("high", "High Performance")
    };

        public string GetCurrentProfile() => _currentProfileId;

        public bool ApplyProfile(string id)
        {
            if (_currentProfileId == id) return false;

            bool isAdmin = _powerService.IsAdministrator();

            if (!isAdmin)
            {
                Console.WriteLine("[Pulse] Running without admin — power plan changes will be skipped");
            }

            _currentProfileId = id;
            Console.WriteLine($"[Pulse] Switched to profile: {id}");

            ApplySystemActions(id, isAdmin);

            return isAdmin;
        }

        private void ApplySystemActions(string profileId, bool isAdmin)
        {
            if (isAdmin)
            {
                var success = _powerService.SetPowerPlan(profileId);

                if (!success)
                {
                    Console.WriteLine("[Pulse Warning] Failed to set power plan");
                }
            }

            var processes = _monitoring.GetHeavyProcesses();

            foreach (var process in processes)
            {
                try
                {
                    if (process.Id == 0 || process.ProcessName.ToLower().Contains("system"))
                        continue;

                    if (profileId == "high")
                        process.PriorityClass = ProcessPriorityClass.AboveNormal;
                    else
                        process.PriorityClass = ProcessPriorityClass.Normal;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Pulse Warning] Failed to change priority for {process.ProcessName}: {ex.Message}");
                }
            }
        }
    }
}
