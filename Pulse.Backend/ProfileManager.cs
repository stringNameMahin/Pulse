using Pulse.Backend.Services;

namespace Pulse.Backend
{
    public class ProfileManager
    {
        private string _currentProfileId = "balanced";

        private readonly MonitoringService _monitoring;
        private readonly PowerPlanService _powerService;
        private readonly ProcessOptimizerService _optimizer;
        private readonly PriorityControlService _priorityControl;
        private readonly UserPriorityService _userPriority;
        private readonly CpuAffinityService _affinity;
        private readonly AffinityControlService _affinityControl;

        public ProfileManager(
            MonitoringService monitoring,
            PowerPlanService powerService,
            ProcessOptimizerService optimizer,
            PriorityControlService priorityControl,
            UserPriorityService userPriority,
            CpuAffinityService affinity,
            AffinityControlService affinityControl)
        {
            _monitoring = monitoring;
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
            bool isSameProfile = _currentProfileId == id;

            bool isAdmin = _powerService.IsAdministrator();

            if (!isAdmin)
            {
                Console.WriteLine("[Pulse] Running without admin — power plan changes will be skipped");
            }

            if (!isSameProfile)
            {
                _currentProfileId = id;
                Console.WriteLine($"[Pulse] Switched to profile: {id}");
            }

            ApplySystemActions(id, isAdmin);
            Console.WriteLine($"[Pulse DEBUG] Requested: {id}, Current: {_currentProfileId}, Admin: {isAdmin}");

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

            var foreground = _optimizer.GetForegroundProcess();
            var apps = _userPriority.GetApps();

            var processedIds = new HashSet<int>();

            if (_priorityControl.IsEnabled())
            {
                if (foreground != null)
                {
                    _optimizer.ApplyPriority(foreground, profileId);
                    processedIds.Add(foreground.Id);
                }
                else
                {
                    Console.WriteLine("[Pulse] No foreground process detected");
                }

                foreach (var process in System.Diagnostics.Process.GetProcesses())
                {
                    try
                    {
                        var name = process.ProcessName.ToLower();

                        if (apps.Contains(name) && !processedIds.Contains(process.Id))
                        {
                            _optimizer.ApplyPriority(process, profileId);
                            processedIds.Add(process.Id);
                        }
                    }
                    catch { }
                }
            }

            if (_affinityControl.IsEnabled())
            {
                var affinityProcessed = new HashSet<int>();

                if (foreground != null)
                {
                    _affinity.ApplyAffinity(foreground, profileId);
                    affinityProcessed.Add(foreground.Id);
                }

                foreach (var process in System.Diagnostics.Process.GetProcesses())
                {
                    try
                    {
                        var name = process.ProcessName.ToLower();

                        if (apps.Contains(name) && !affinityProcessed.Contains(process.Id))
                        {
                            _affinity.ApplyAffinity(process, profileId);
                            affinityProcessed.Add(process.Id);
                        }
                    }
                    catch { }
                }
            }
        }
    }
}