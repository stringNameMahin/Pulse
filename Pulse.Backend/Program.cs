var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Basic health/status endpoint
app.MapGet("/status", () =>
{
    return Results.Ok(new
    {
        app = "Pulse Backend",
        status = "running",
        time = DateTime.Now
    });
});

app.Run();
