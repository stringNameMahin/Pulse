using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

PerformanceCounter? cpuCounter = null;

if (OperatingSystem.IsWindows())
{
    cpuCounter = new PerformanceCounter(
        "Processor",
        "% Processor Time",
        "_Total"
    );

    cpuCounter.NextValue(); // warm-up
}

app.MapGet("/status", () =>
{
    float cpuUsage = 0;

    if (OperatingSystem.IsWindows() && cpuCounter != null)
    {
        cpuUsage = cpuCounter.NextValue();
    }

    var gcInfo = GC.GetGCMemoryInfo();
    double totalRamMb = gcInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0);
    double usedRamMb = Environment.WorkingSet / (1024.0 * 1024.0);

    return Results.Ok(new
    {
        app = "Pulse Backend",
        platform = OperatingSystem.IsWindows() ? "Windows" : "Unsupported",
        cpuUsagePercent = Math.Round(cpuUsage, 2),
        ramUsedMB = Math.Round(usedRamMb, 2),
        ramTotalMB = Math.Round(totalRamMb, 2),
        time = DateTime.Now
    });
});

app.Run();
