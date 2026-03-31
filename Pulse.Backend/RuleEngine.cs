using Pulse.Backend.Services;

namespace Pulse.Backend
{
    public class RuleEngine
    {
        private readonly MonitoringService _monitoring;

        public RuleEngine(MonitoringService monitoring)
        {
            _monitoring = monitoring;
        }

        public string DecideProfile()
        {
            return _monitoring.IsHeavyLoad() ? "high" : "balanced";
        }
    }
}
