using Pulse.Backend.Services;
using System.Diagnostics;
using System.Text.Json;

namespace Pulse.Backend
{
    public class RuleEngine
    {
        private readonly MonitoringService _monitoring;
        private readonly string _filePath = "rules.json";

        private List<Rule> _rules = new();

        public RuleEngine(MonitoringService monitoring)
        {
            _monitoring = monitoring;
            LoadRules();
        }

        public List<Rule> GetRules() => _rules;

        public void AddRule(Rule rule)
        {
            _rules.Add(rule);
            SaveRules();

            Console.WriteLine($"[Pulse] Rule added: {rule.TriggerProcess}");
        }

        private void LoadRules()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return;

                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<List<Rule>>(json);

                if (data != null)
                    _rules = data;

                Console.WriteLine($"[Pulse] Loaded {_rules.Count} rules");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Pulse Warning] Failed to load rules: {ex.Message}");
            }
        }

        private void SaveRules()
        {
            try
            {
                var json = JsonSerializer.Serialize(_rules, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Pulse Warning] Failed to save rules: {ex.Message}");
            }
        }

        public string DecideProfile()
        {
            var running = Process.GetProcesses()
                .Select(p => p.ProcessName.ToLower())
                .ToHashSet();

            foreach (var rule in _rules)
            {
                if (running.Contains(rule.TriggerProcess.ToLower()))
                {
                    Console.WriteLine($"[Pulse] Rule triggered: {rule.TriggerProcess}");
                    return rule.TargetProfile;
                }
            }

            return _monitoring.IsHeavyLoad() ? "high" : "balanced";
        }

        public List<string> GetAppsToClose()
        {
            var running = Process.GetProcesses()
                .Select(p => p.ProcessName.ToLower())
                .ToHashSet();

            foreach (var rule in _rules)
            {
                if (running.Contains(rule.TriggerProcess.ToLower()))
                {
                    return rule.CloseApps;
                }
            }

            return new List<string>();
        }
    }
}