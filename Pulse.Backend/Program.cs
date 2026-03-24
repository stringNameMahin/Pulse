using Pulse.Backend;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MonitoringService>();
builder.Services.AddSingleton<RuleEngine>();
builder.Services.AddSingleton<ProfileManager>();
builder.Services.AddHostedService<AutoSwitchService>();
builder.Services.AddSingleton<AutoModeService>();

//CORS fix
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors();

PerformanceCounter? cpuCounter = null;

if (OperatingSystem.IsWindows())
{
    cpuCounter = new PerformanceCounter(
        "Processor",
        "% Processor Time",
        "_Total"
    );

    cpuCounter.NextValue(); 
}

app.MapGet("/profiles", (ProfileManager pm) =>
{
    return Results.Ok(new
    {
        currentProfileId = pm.GetCurrentProfile(),
        profiles = pm.Profiles
    });
});

app.MapPost("/apply-profile/{id}", (string id, ProfileManager pm) =>
{
    var profile = pm.Profiles.FirstOrDefault(p => p.Id == id);

    if (profile == null)
        return Results.NotFound("Profile not found");

    var changed = pm.ApplyProfile(profile.Id);

    return Results.Ok(new
    {
        message = changed ? $"Profile '{profile.Name}' applied" : "Already active",
        currentProfileId = pm.GetCurrentProfile()
    });

});

app.MapPost("/auto-switch", (RuleEngine rule, ProfileManager pm) =>
{
    var decided = rule.DecideProfile();
    pm.ApplyProfile(decided);

    return Results.Ok(new
    {
        currentProfileId = pm.GetCurrentProfile()
    });
});
app.MapPost("/auto-mode/{state}", (string state, AutoModeService auto) =>
{
    bool enable = state.Equals("on", StringComparison.OrdinalIgnoreCase);
    auto.SetEnabled(enable);

    return Results.Ok(new
    {
        autoMode = enable
    });
});


app.MapGet("/status", async() =>
{
    float cpuUsage = 0;

    if (OperatingSystem.IsWindows() && cpuCounter != null)
    {
        await Task.Delay(100);
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
    var processList = new List<ProcessInfo>();

    foreach (var process in Process.GetProcesses())
    {
        try
        {
            processList.Add(new ProcessInfo
            {
                Name = process.ProcessName,
                Id = process.Id,
                CpuTimeSeconds = Math.Round(process.TotalProcessorTime.TotalSeconds, 2),
                MemoryMB = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2)
            });
        }
        catch { }
    }

    return Results.Ok(
        processList.OrderByDescending(p => p.MemoryMB)
        .Take(20)
    );
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
app.MapGet("/auto-mode", (AutoModeService auto) =>
{
    return Results.Ok(new
    {
        enabled = auto.IsEnabled()
    });
});



app.Run();