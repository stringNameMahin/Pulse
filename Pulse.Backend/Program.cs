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

app.MapGet("/processes", () =>
{
    var processList = new List<object>();

    foreach (var process in Process.GetProcesses())
    {
        try
        {
            processList.Add(new
            {
                name = process.ProcessName,
                id = process.Id,
                cpuTimeSeconds = Math.Round(process.TotalProcessorTime.TotalSeconds, 2),
                memoryMB = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2)
            });
        }
        catch
        {
            // Some system processes deny access – ignore safely
        }
    }

    return Results.Ok(processList);
});


app.MapGet("/heavy-processes", () =>
{
    const long memoryThresholdMb = 500;
    var heavyProcesses = new List<object>();

    foreach (var process in Process.GetProcesses())
    {
        try
        {
            double memoryMb = process.WorkingSet64 / (1024.0 * 1024.0);

            if (memoryMb >= memoryThresholdMb)
            {
                heavyProcesses.Add(new
                {
                    name = process.ProcessName,
                    id = process.Id,
                    memoryMB = Math.Round(memoryMb, 2)
                });
            }
        }
        catch
        {
            // Ignore protected processes
        }
    }

    return Results.Ok(new
    {
        thresholdMB = memoryThresholdMb,
        heavyProcesses
    });
});


app.Run();