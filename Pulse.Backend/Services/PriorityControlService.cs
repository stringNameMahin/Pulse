namespace Pulse.Backend.Services
{
    public class PriorityControlService
    {
        private bool _enabled = false; // 🔥 OFF by default (safe)

        public bool IsEnabled() => _enabled;

        public void SetEnabled(bool value)
        {
            _enabled = value;
            Console.WriteLine($"[Pulse] Smart Priority: {(value ? "ON" : "OFF")}");
        }
    }
}