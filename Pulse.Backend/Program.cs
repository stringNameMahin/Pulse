using Microsoft.Win32;
using System.Runtime.InteropServices;
using Pulse.Backend;
using Pulse.Backend.Services;
using System.Data;
using System.Diagnostics;
using Rule = Pulse.Backend.Rule;

var builder = WebApplication.CreateBuilder(args);

// Services (CORE)
builder.Services.AddSingleton<MonitoringService>();
builder.Services.AddSingleton<RuleEngine>();
builder.Services.AddSingleton<ProfileManager>();

// Services (CONTROL)
builder.Services.AddSingleton<AutoModeService>();
builder.Services.AddSingleton<PriorityControlService>();
builder.Services.AddSingleton<AffinityControlService>();
builder.Services.AddSingleton<TerminationControlService>();

// Services (SYSTEM)
builder.Services.AddSingleton<PowerPlanService>();
builder.Services.AddSingleton<ProcessOptimizerService>();
builder.Services.AddSingleton<UserPriorityService>();
builder.Services.AddSingleton<CpuAffinityService>();
builder.Services.AddSingleton<ProcessTerminationService>();

// Background Services
builder.Services.AddHostedService<AutoSwitchService>();

// EVENT SERVICE
builder.Services.AddSingleton<EventService>();

// CORS
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

// HEALTH CHECK
app.MapGet("/status", () =>
{
    float cpuUsage = 0;

    try
    {
        using var cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");

        cpuCounter.NextValue();
        Thread.Sleep(500);

        cpuUsage = cpuCounter.NextValue();
    }
    catch
    {
        cpuUsage = 0;
    }

    var (totalMb, availableMb) = MemoryHelper.GetMemory();
    var usedMb = totalMb - availableMb;

    return Results.Ok(new
    {
        cpuUsagePercent = Math.Round(cpuUsage, 2),
        ramUsedMB = Math.Round(usedMb, 2),
        ramTotalMB = Math.Round(totalMb, 2)
    });
});

// RULES
app.MapGet("/rules", (RuleEngine engine) =>
{
    return Results.Ok(engine.GetRules());
});

app.MapPost("/rules", (Rule rule, RuleEngine engine) =>
{
    if (string.IsNullOrWhiteSpace(rule.TriggerProcess))
        return Results.BadRequest("Trigger process required");

    if (!engine.AddRule(rule))
        return Results.BadRequest("Duplicate rule");

    return Results.Ok(engine.GetRules());
});

app.MapDelete("/rules/{id}", (Guid id, RuleEngine engine) =>
{
    if (!engine.DeleteRule(id))
        return Results.NotFound("Rule not found");

    return Results.Ok(engine.GetRules());
});

// AUTO MODE
app.MapGet("/auto-mode", (AutoModeService auto) =>
{
    return Results.Ok(new { enabled = auto.IsEnabled() });
});

app.MapPost("/auto-mode/{state}", (string state, AutoModeService auto) =>
{
    bool enable = state.Equals("on", StringComparison.OrdinalIgnoreCase);
    auto.SetEnabled(enable);

    return Results.Ok(new { autoMode = enable });
});

// PRIORITY MODE
app.MapGet("/priority-mode", (PriorityControlService pcs) =>
{
    return Results.Ok(new { enabled = pcs.IsEnabled() });
});

app.MapPost("/priority-mode/{state}", (string state, PriorityControlService pcs) =>
{
    bool enable = state.Equals("on", StringComparison.OrdinalIgnoreCase);
    pcs.SetEnabled(enable);

    return Results.Ok(new { priorityMode = enable });
});

// PRIORITY APPS
app.MapGet("/priority-apps", (UserPriorityService ups) =>
{
    return Results.Ok(ups.GetApps());
});

app.MapPost("/priority-apps", (List<string> apps, UserPriorityService ups) =>
{
    ups.SetApps(apps);
    return Results.Ok(ups.GetApps());
});

// PROFILES
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

    var applied = pm.ApplyProfile(id);

    return Results.Ok(new
    {
        success = true,
        requiresAdmin = !applied,
        currentProfileId = pm.GetCurrentProfile()
    });
});

// EVENT SERVICE
app.MapGet("/events", (EventService events) =>
{
    return Results.Ok(events.GetAndClear());
});

app.MapGet("/processes", () =>
{
    var list = new List<object>();

    foreach (var p in System.Diagnostics.Process.GetProcesses())
    {
        try
        {
            list.Add(new
            {
                name = p.ProcessName,
                id = p.Id,
                memoryMB = p.WorkingSet64 / (1024 * 1024),
                cpuTimeSeconds = p.TotalProcessorTime.TotalSeconds
            });
        }
        catch { }
    }

    return Results.Ok(list);
});

app.Run();