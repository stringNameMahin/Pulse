namespace Pulse.Backend
{
    public class Rule
    {
        public string TriggerProcess { get; set; } = "";
        public string TargetProfile { get; set; } = "balanced";
        public List<string> CloseApps { get; set; } = new();
    }
}