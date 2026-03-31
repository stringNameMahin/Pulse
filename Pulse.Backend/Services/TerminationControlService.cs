namespace Pulse.Backend.Services
{
    public class TerminationControlService
    {
        private string _mode = "safe"; // safe | smart | aggressive

        public string GetMode() => _mode;

        public void SetMode(string mode)
        {
            _mode = mode.ToLower();
            Console.WriteLine($"[Pulse] Termination Mode: {_mode}");
        }
    }
}