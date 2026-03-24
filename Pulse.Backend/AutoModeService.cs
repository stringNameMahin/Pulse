namespace Pulse.Backend
{
    public class AutoModeService
    {
        private bool _isEnabled = true;

        public bool IsEnabled() => _isEnabled;

        public void SetEnabled(bool value)
        {
            _isEnabled = value;
            Console.WriteLine($"[Pulse] Auto Mode: {(value ? "ON" : "OFF")}");
        }
    }
}
