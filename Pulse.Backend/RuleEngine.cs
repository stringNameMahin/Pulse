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

        public bool AddRule(Rule rule)
        {
            NormalizeRule(rule);

            if (IsDuplicate(rule))
                return false;

            _rules.Add(rule);
            SaveRules();

            Console.WriteLine($"[Pulse] Rule added: {rule.TriggerProcess}");
            return true;
        }

        public bool UpdateRule(Guid id, Rule updatedRule)
        {
            var existing = _rules.FirstOrDefault(r => r.Id == id);
            if (existing == null)
                return false;

            NormalizeRule(updatedRule);

            if (_rules.Any(r =>
                r.Id != id &&
                AreRulesEqual(r, updatedRule)))
            {
                return false;
            }

            existing.TriggerProcess = updatedRule.TriggerProcess;
            existing.TargetProfile = updatedRule.TargetProfile;
            existing.CloseApps = updatedRule.CloseApps;

            SaveRules();
            return true;
        }

        public bool DeleteRule(Guid id)
        {
            var rule = _rules.FirstOrDefault(r => r.Id == id);
            if (rule == null)
                return false;

            _rules.Remove(rule);
            SaveRules();
            return true;
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
                {
                    foreach (var rule in data)
                    {
                        if (rule.Id == Guid.Empty)
                            rule.Id = Guid.NewGuid();

                        NormalizeRule(rule);
                    }

                    _rules = data;
                }

                SaveRules();

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

        public void SaveAfterExternalChange()
        {
            SaveRules();
        }

        public string DecideProfile()
        {
            var running = Process.GetProcesses()
                .Select(p => p.ProcessName.ToLower())
                .ToHashSet();

            foreach (var rule in _rules)
            {
                if (running.Contains(rule.TriggerProcess))
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
                if (running.Contains(rule.TriggerProcess))
                {
                    return rule.CloseApps;
                }
            }

            return new List<string>();
        }


        private void NormalizeRule(Rule rule)
        {
            rule.TriggerProcess = rule.TriggerProcess.ToLower().Trim();
            rule.TargetProfile = rule.TargetProfile.ToLower().Trim();
            rule.CloseApps = rule.CloseApps
                .Select(a => a.ToLower().Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct()
                .ToList();
        }

        private bool IsDuplicate(Rule rule)
        {
            return _rules.Any(r => AreRulesEqual(r, rule));
        }

        private bool AreRulesEqual(Rule a, Rule b)
        {
            return a.TriggerProcess == b.TriggerProcess &&
                   a.TargetProfile == b.TargetProfile &&
                   new HashSet<string>(a.CloseApps).SetEquals(b.CloseApps);
        }
    }
}