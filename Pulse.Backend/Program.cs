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
    cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    cpuCounter.NextValue();
}

// rules

app.MapPost("/rules", (Rule rule, RuleEngine engine) =>
{
    if (string.IsNullOrWhiteSpace(rule.TriggerProcess))
        return Results.BadRequest("Trigger process required");

    if (!engine.AddRule(rule))
        return Results.BadRequest("Duplicate rule");

    return Results.Ok(engine.GetRules());
});

app.MapGet("/rules", (RuleEngine engine) =>
{
    return Results.Ok(engine.GetRules());
});

app.MapDelete("/rules/{id}", (Guid id, RuleEngine engine) =>
{
    if (!engine.DeleteRule(id))
        return Results.NotFound("Rule not found");

    return Results.Ok(engine.GetRules());
});

// mode

app.MapPost("/auto-mode/{state}", (string state, AutoModeService auto) =>
{
    bool enable = state.Equals("on", StringComparison.OrdinalIgnoreCase);
    auto.SetEnabled(enable);

    return Results.Ok(new { autoMode = enable });
});

app.MapGet("/auto-mode", (AutoModeService auto) =>
{
    return Results.Ok(new { enabled = auto.IsEnabled() });
});

// priority

app.MapPost("/priority-mode/{state}", (string state, PriorityControlService pcs) =>
{
    bool enable = state.Equals("on", StringComparison.OrdinalIgnoreCase);
    pcs.SetEnabled(enable);

    return Results.Ok(new { priorityMode = enable });
});

app.MapGet("/priority-mode", (PriorityControlService pcs) =>
{
    return Results.Ok(new { enabled = pcs.IsEnabled() });
});

// pri. apps

app.MapGet("/priority-apps", (UserPriorityService ups) =>
{
    return Results.Ok(ups.GetApps());
});

app.MapPost("/priority-apps", (List<string> apps, UserPriorityService ups) =>
{
    ups.SetApps(apps);
    return Results.Ok(ups.GetApps());
});

// prof

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

// auto switch

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
            if (appsToClose.Contains(process.ProcessName.ToLower()))
            {
                pts.TryCloseProcess(process, foreground);
            }
        }
        catch { }
    }

    return Results.Ok(new { currentProfileId = pm.GetCurrentProfile() });
});

app.Run();