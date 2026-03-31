namespace Pulse.Backend.Services
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    public class AutoSwitchService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AutoSwitchService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[Pulse] AutoSwitch Service Started...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var autoMode = scope.ServiceProvider.GetRequiredService<AutoModeService>();

                    if (!autoMode.IsEnabled())
                    {
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }

                    var ruleEngine = scope.ServiceProvider.GetRequiredService<RuleEngine>();
                    var profileManager = scope.ServiceProvider.GetRequiredService<ProfileManager>();
                    var decidedProfile = ruleEngine.DecideProfile();

                    profileManager.ApplyProfile(decidedProfile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Pulse ERROR] {ex.Message}");
                }

                await Task.Delay(5000, stoppingToken); // 5 sec loop
            }
        }
    }
}
