using Pulse.Backend.Services;

namespace Pulse.Backend
{
    public class ProfileManager
    {
        private string _currentProfileId = "balanced";

        private readonly PowerPlanService _powerService;
        private readonly ProcessOptimizerService _optimizer;
        private readonly PriorityControlService _priorityControl;
        private readonly UserPriorityService _userPriority;
        private readonly CpuAffinityService _affinity;
        private readonly AffinityControlService _affinityControl;

        public ProfileManager(
            PowerPlanService powerService,
            ProcessOptimizerService optimizer,
            PriorityControlService priorityControl,
            UserPriorityService userPriority,
            CpuAffinityService affinity,
            AffinityControlService affinityControl)
        {
            _powerService = powerService;
            _optimizer = optimizer;
            _priorityControl = priorityControl;
            _userPriority = userPriority;
            _affinity = affinity;
            _affinityControl = affinityControl;
        }

        public List<PerformanceProfile> Profiles { get; } = new()
        {
            new("balanced", "Balanced"),
            new("high", "High Performance")
        };

        public string GetCurrentProfile() => _currentProfileId;

        public bool ApplyProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            bool isAdmin = _powerService.IsAdministrator();

            if (_currentProfileId != id)
            {
                _currentProfileId = id;
                Console.WriteLine($"[Pulse] Switched to profile: {id}");
            }

            ApplySystemActions(id, isAdmin);

            return isAdmin;
        }

        private void ApplySystemActions(string profileId, bool isAdmin)
        {
            // POWER PLAN
            if (isAdmin)
            {
                var success = _powerService.SetPowerPlan(profileId);

                if (!success)
                {
                    Console.WriteLine("[Pulse Warning] Failed to set power plan");
                }
            }

            // PRIORITY

            if (_priorityControl.IsEnabled())
            {
                var foreground = _optimizer.GetForegroundProcess();
                var apps = _userPriority.GetApps();
                var processed = new HashSet<int>();

                if (foreground != null)
                {
                    _optimizer.ApplyPriority(foreground, profileId);
                    processed.Add(foreground.Id);
                }

                foreach (var process in System.Diagnostics.Process.GetProcesses())
                {
                    try
                    {
                        var name = process.ProcessName.ToLower();

                        if (apps.Contains(name) && !processed.Contains(process.Id))
                        {
                            _optimizer.ApplyPriority(process, profileId);
                            processed.Add(process.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Pulse Warning] Priority failed: {ex.Message}");
                    }
                }
            }

            // AFFINITY

            if (_affinityControl.IsEnabled())
            {
                var foreground = _optimizer.GetForegroundProcess();
                var apps = _userPriority.GetApps();
                var processed = new HashSet<int>();

                if (foreground != null)
                {
                    _affinity.ApplyAffinity(foreground, profileId);
                    processed.Add(foreground.Id);
                }

                foreach (var process in System.Diagnostics.Process.GetProcesses())
                {
                    try
                    {
                        var name = process.ProcessName.ToLower();

                        if (apps.Contains(name) && !processed.Contains(process.Id))
                        {
                            _affinity.ApplyAffinity(process, profileId);
                            processed.Add(process.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Pulse Warning] Affinity failed: {ex.Message}");
                    }
                }
            }
        }
    }
}