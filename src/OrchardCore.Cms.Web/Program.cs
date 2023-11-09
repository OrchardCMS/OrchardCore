using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

builder.Services.AddHealthChecks().AddDiskStorageHealthCheck(setup => setup.CheckAllDrives = true);
builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(15); // time in seconds between check
    setup.MaximumHistoryEntriesPerEndpoint(60); // maximum history of checks
    setup.SetApiMaxActiveRequests(1);
    setup.SetHeaderText("Health");
    setup.AddHealthCheckEndpoint("diskstorage", "/health.json");
    setup.AddHealthCheckEndpoint("test", "/health/live");

    ////setup.AddWebhookNotification("webhook1", uri: "https://healthchecks.requestcatcher.com/",
    ////                            payload: "{ message: \"Webhook report for [[LIVENESS]]: [[FAILURE]] - Description: [[DESCRIPTIONS]]\"}",
    ////                            restorePayload: "{ message: \"[[LIVENESS]] is back to life\"}");
}).AddInMemoryStorage();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseOrchardCore();

app.MapHealthChecks("/health.json", new HealthCheckOptions
{
    AllowCachingResponses = false,
    ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                },
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.UseHealthChecksUI();

app.Run();
