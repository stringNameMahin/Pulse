namespace Pulse.Backend.Services
{
    public class AffinityControlService
    {
        private bool _enabled = false;

        public bool IsEnabled() => _enabled;

        public void SetEnabled(bool value)
        {
            _enabled = value;
            Console.WriteLine($"[Pulse] CPU Affinity: {(value ? "ON" : "OFF")}");
        }
    }
}