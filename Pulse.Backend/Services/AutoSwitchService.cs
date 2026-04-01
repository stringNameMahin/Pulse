using Microsoft.Extensions.Hosting;

namespace Pulse.Backend.Services
{
    public class AutoSwitchService : BackgroundService
    {
        private readonly AutoModeService _autoMode;
        private readonly RuleEngine _ruleEngine;
        private readonly ProfileManager _profileManager;
        private readonly ProcessTerminationService _termination;
        private readonly ProcessOptimizerService _optimizer;
        private readonly EventService _events;
        private string _lastProfile = "";

        public AutoSwitchService(
            AutoModeService autoMode,
            RuleEngine ruleEngine,
            ProfileManager profileManager,
            ProcessTerminationService termination,
            ProcessOptimizerService optimizer,
            EventService events)
        {
            _autoMode = autoMode;
            _ruleEngine = ruleEngine;
            _profileManager = profileManager;
            _termination = termination;
            _optimizer = optimizer;
            _events = events;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[Pulse] AutoSwitch Service Started...");


            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(2000, stoppingToken);

                    if (!_autoMode.IsEnabled() || _autoMode.IsInManualOverride())
                        continue;

                    // ----------------------
                    // Decide Profile
                    // ----------------------

                    var decidedProfile = _ruleEngine.DecideProfile();


                    // Only log if profile actually changed
                    _profileManager.ApplyProfile(decidedProfile);
                    if (_lastProfile != decidedProfile)
                    {
                        _events.Add($"Profile switched to {decidedProfile}");
                        _lastProfile = decidedProfile;
                    }


                    // ----------------------
                    // Handle Rule Actions
                    // ----------------------
                    var appsToClose = _ruleEngine.GetAppsToClose();
                    var foreground = _optimizer.GetForegroundProcess();

                    if (appsToClose.Count > 0)
                    {
                        var handled = new HashSet<string>();

                        foreach (var process in System.Diagnostics.Process.GetProcesses())
                        {
                            try
                            {
                                var name = process.ProcessName.ToLower();

                                if (appsToClose.Contains(name) && !handled.Contains(name))
                                {
                                    _termination.TryCloseProcess(process, foreground);
                                    _events.Add($"Closed {name}");
                                    handled.Add(name); // 🔥 only once per app
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Pulse Warning] Termination failed: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Pulse ERROR] {ex.Message}");
                }
            }
        }
    }
}