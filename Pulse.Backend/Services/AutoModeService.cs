namespace Pulse.Backend.Services
{
    public class AutoModeService
    {
        private bool _isEnabled = true;
        private DateTime _lastManualOverride = DateTime.MinValue;

        public void TriggerManualOverride()
        {
            _lastManualOverride = DateTime.Now;
        }

        public bool IsInManualOverride()
        {
            return (DateTime.Now - _lastManualOverride).TotalSeconds < 5;
        }

        public bool IsEnabled() => _isEnabled;

        public void SetEnabled(bool value)
        {
            _isEnabled = value;
            Console.WriteLine($"[Pulse] Auto Mode: {(value ? "ON" : "OFF")}");
        }
    }
}