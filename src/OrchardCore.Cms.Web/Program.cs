using OrchardCore.Clusters;
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

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
