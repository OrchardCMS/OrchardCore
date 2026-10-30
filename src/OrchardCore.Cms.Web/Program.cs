using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup")
    // Exports traces, metrics and logs once an OTLP endpoint is configured, e.g. with the 'OTEL_EXPORTER_OTLP_ENDPOINT' environment variable.
    // See https://docs.orchardcore.net/en/latest/reference/modules/OpenTelemetry/.
    .AddOpenTelemetry();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseOrchardCore();

await app.RunAsync();
