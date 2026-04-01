namespace Pulse.Backend.Services
{
    public class TerminationControlService
    {
        private string _mode = "safe";

        public string GetMode() => _mode;

        public void SetMode(string mode)
        {
            _mode = mode.ToLower();
            Console.WriteLine($"[Pulse] Termination Mode: {_mode}");
        }
    }
}