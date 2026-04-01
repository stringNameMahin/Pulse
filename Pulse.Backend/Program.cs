using Pulse.Backend;
using Pulse.Backend.Services;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MonitoringService>();
builder.Services.AddSingleton<RuleEngine>();
builder.Services.AddSingleton<ProfileManager>();
builder.Services.AddHostedService<AutoSwitchService>();
builder.Services.AddSingleton<AutoModeService>();
builder.Services.AddSingleton<PowerPlanService>();
builder.Services.AddSingleton<ProcessOptimizerService>();
builder.Services.AddSingleton<PriorityControlService>();
builder.Services.AddSingleton<UserPriorityService>();
builder.Services.AddSingleton<CpuAffinityService>();
builder.Services.AddSingleton<AffinityControlService>();
builder.Services.AddSingleton<ProcessTerminationService>();
builder.Services.AddSingleton<TerminationControlService>();

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

app.MapPost("/apply-profile/{id}", (string id, ProfileManager pm, AutoModeService auto) =>
{
    auto.TriggerManualOverride();

    var profile = pm.Profiles.FirstOrDefault(p => p.Id == id);

    if (profile == null)
        return Results.NotFound("Profile not found");

    var appliedWithAdmin = pm.ApplyProfile(profile.Id);

    return Results.Ok(new
    {
        success = true,
        requiresAdmin = !appliedWithAdmin,
        message = appliedWithAdmin
            ? $"Profile '{profile.Name}' applied"
            : "Profile applied (limited mode - admin required for full optimization)",
        currentProfileId = pm.GetCurrentProfile()
    });
});

app.MapPost("/auto-switch", (
    RuleEngine rule,
    ProfileManager pm,
    AutoModeService auto,
    ProcessTerminationService pts,
    ProcessOptimizerService optimizer) =>
{
    if (auto.IsInManualOverride())
        return Results.Ok(new { skipped = true });

    var decided = rule.DecideProfile();
    pm.ApplyProfile(decided);

    var appsToClose = rule.GetAppsToClose();
    var foreground = optimizer.GetForegroundProcess();

    foreach (var process in Process.GetProcesses())
    {
        try
        {
            var name = process.ProcessName.ToLower();

            if (appsToClose.Contains(name))
            {
                if (pts.TryCloseProcess(process, foreground))
                {
                    Console.WriteLine($"[Pulse] Rule safely closed: {process.ProcessName}");
                }
            }
        }
        catch { }
    }

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
app.MapPost("/priority-mode/{state}", (string state, PriorityControlService pcs) =>
{
    bool enable = state.Equals("on", StringComparison.OrdinalIgnoreCase);
    pcs.SetEnabled(enable);

    return Results.Ok(new
    {
        priorityMode = enable
    });
});

app.MapGet("/priority-mode", (PriorityControlService pcs) =>
{
    return Results.Ok(new
    {
        enabled = pcs.IsEnabled()
    });
});
app.MapGet("/priority-apps", (UserPriorityService ups) =>
{
    return Results.Ok(ups.GetApps());
});
app.MapPost("/priority-apps", (List<string> apps, UserPriorityService ups) =>
{
    Console.WriteLine("[Pulse] Updating priority apps...");
    ups.SetApps(apps);

    return Results.Ok(new
    {
        apps = ups.GetApps()
    });
});
app.MapPost("/affinity-mode/{state}", (string state, AffinityControlService acs) =>
{
    bool enable = state.Equals("on", StringComparison.OrdinalIgnoreCase);
    acs.SetEnabled(enable);

    return Results.Ok(new { affinityMode = enable });
});

app.MapGet("/affinity-mode", (AffinityControlService acs) =>
{
    return Results.Ok(new { enabled = acs.IsEnabled() });
});
app.MapPost("/termination-mode/{mode}", (string mode, TerminationControlService tcs) =>
{
    tcs.SetMode(mode);
    return Results.Ok(new { mode });
});
app.MapGet("/termination-mode", (TerminationControlService tcs) =>
{
    return Results.Ok(new { mode = tcs.GetMode() });
});
app.MapPost("/cleanup", (
    ProcessTerminationService pts,
    TerminationControlService tcs,
    ProcessOptimizerService optimizer) =>
{
    var heavy = pts.GetHeavyProcesses();
    var closed = new List<string>();

    var mode = tcs.GetMode();
    var foreground = optimizer.GetForegroundProcess();

    Console.WriteLine($"[Pulse] Cleanup triggered | Mode: {mode} | Processes: {heavy.Count}");

    foreach (var process in heavy)
    {
        if (mode == "safe")
            continue;

        if (mode == "smart")
        {
            if (foreground != null && process.Id == foreground.Id)
                continue;
        }

        if (pts.TryCloseProcess(process, foreground))
        {
            closed.Add(process.ProcessName);
        }
    }

    return Results.Ok(new
    {
        closed
    });
});
app.MapPost("/rules", (Rule rule, RuleEngine engine) =>
{
    var success = engine.AddRule(rule);

    if (!success)
        return Results.BadRequest("Duplicate rule");

    return Results.Ok(engine.GetRules());
});
app.MapGet("/rules", (RuleEngine engine) =>
{
    return Results.Ok(engine.GetRules());
});
app.MapDelete("/rules/{id}", (Guid id, RuleEngine engine) =>
{
    var success = engine.DeleteRule(id);

    if (!success)
        return Results.NotFound("Rule not found");

    return Results.Ok(engine.GetRules());
});
app.MapPut("/rules/{id}", (Guid id, Rule updatedRule, RuleEngine engine) =>
{
    var success = engine.UpdateRule(id, updatedRule);

    if (!success)
        return Results.BadRequest("Update failed (duplicate or not found)");

    return Results.Ok(engine.GetRules());
});

app.Run();