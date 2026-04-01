using System.Diagnostics;

namespace Pulse.Backend.Services
{
    public class UserPriorityService
    {
        private readonly List<string> _priorityApps = new();

        public List<string> GetApps() => _priorityApps;

        public void SetApps(List<string> apps)
        {
            _priorityApps.Clear();

            foreach (var app in apps)
            {
                if (string.IsNullOrWhiteSpace(app)) continue;

                var normalized = app.ToLower().Trim();
                var resolved = ResolveProcessName(normalized);

                if (!_priorityApps.Contains(resolved))
                {
                    _priorityApps.Add(resolved);
                }
            }

            Console.WriteLine($"[Pulse] Priority apps updated: {string.Join(", ", _priorityApps)}");
        }

        private string ResolveProcessName(string input)
        {
            var query = input.ToLower().Trim();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    var name = process.ProcessName.ToLower();
                    var title = process.MainWindowTitle?.ToLower() ?? "";

                    if (name.Contains(query) || title.Contains(query))
                    {
                        Console.WriteLine($"[Pulse] Resolved '{input}' → '{name}'");
                        return name;
                    }
                }
                catch { }
            }

            Console.WriteLine($"[Pulse] Could not resolve '{input}', using as-is");
            return query;
        }
    }
}