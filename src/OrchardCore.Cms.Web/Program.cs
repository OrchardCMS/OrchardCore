using OrchardCore.Clusters;
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

// Tenant clustering is only wired when its configuration section exists on start.
var clustersSection = builder.Configuration.GetSection(ClustersOptions.ConfigurationSection);
var useTenantClusters = clustersSection.Exists();

if (useTenantClusters)
{
    builder.Services
        .AddReverseProxy()
        .AddTenantClusters()
        .LoadFromConfig(clustersSection);
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

// Also without this section, as this node may still receive forwarded requests, whose secret should not reach a tenant.
app.UseTenantClusterRouting();

app.UseOrchardCore();

if (useTenantClusters)
{
    app.MapReverseProxy(proxyPipeline =>
    {
        proxyPipeline
            .UseTenantClusters()
            .UseSessionAffinity()
            .UseLoadBalancing()
            .UsePassiveHealthChecks();
    });
}

await app.RunAsync();
