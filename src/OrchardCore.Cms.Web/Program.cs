using OrchardCore.Clusters;
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

builder.Services
    .AddReverseProxy()
    .AddTenantClusters()
    .LoadFromConfig(builder.Configuration.GetSection(ClustersOptions.ConfigurationSection));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseTenantClusterRouting();

app.UseOrchardCore();

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline
        .UseTenantClusters()
        .UseSessionAffinity()
        .UseLoadBalancing()
        .UsePassiveHealthChecks();
});

await app.RunAsync();
