using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

var orchardCore = builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

// Exports traces, metrics and logs when an OTLP endpoint is configured, e.g. with the 'OTEL_EXPORTER_OTLP_ENDPOINT' environment variable.
// Without one nothing would be exported, so nothing is collected.
// See https://docs.orchardcore.net/en/latest/reference/modules/OpenTelemetry/.
if (builder.Configuration.HasOtlpEndpoint())
{
    orchardCore.AddOpenTelemetry();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseOrchardCore();

await app.RunAsync();
