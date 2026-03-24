using System.Diagnostics;

namespace Pulse.Backend
{
    public class ProfileManager
    {
        private string _currentProfileId = "balanced";
        private readonly MonitoringService _monitoring;

        public ProfileManager(MonitoringService monitoring)
        {
            _monitoring = monitoring;
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

            _currentProfileId = id;
            Console.WriteLine($"[Pulse] Switched to profile: {id}");

            ApplySystemActions(id);

            return true;
        }

        private void ApplySystemActions(string profileId)
        {
            var processes = _monitoring.GetHeavyProcesses();

            foreach (var process in processes)
            {
                try
                {
                    // skip critical/system processes (ps: was crashing)
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
