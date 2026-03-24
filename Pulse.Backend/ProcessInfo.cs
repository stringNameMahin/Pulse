namespace Pulse.Backend
{
    public class ProcessInfo
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public double CpuTimeSeconds { get; set; }
        public double MemoryMB { get; set; }
    }
}
