namespace Pulse.Backend.Services
{
    public class EventService
    {
        private readonly List<string> _events = new();
        private readonly object _lock = new();

        public void Add(string message)
        {
            lock (_lock)
            {
                _events.Add($"[{DateTime.Now:HH:mm:ss}] {message}");

                // limit memory (keep last 50)
                if (_events.Count > 50)
                    _events.RemoveAt(0);
            }
        }

        public List<string> GetAll()
        {
            lock (_lock)
            {
                return _events.ToList();
            }
        }

        public List<string> GetAndClear()
        {
            lock (_lock)
            {
                var copy = _events.ToList();
                _events.Clear();
                return copy;
            }
        }
    }
}