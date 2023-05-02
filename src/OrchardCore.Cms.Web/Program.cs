using OrchardCore.Clusters;
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

if (builder.Configuration.UseAsClustersProxy())
{
    builder.Services
        .AddReverseProxy()
        .AddClusters()
        .LoadFromConfig(builder.Configuration.GetClustersSection())
        ;
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseOrchardCore();

if (app.Configuration.UseAsClustersProxy())
{
    app.MapReverseProxy(proxyPipeline =>
    {
        proxyPipeline
            .UseClusters()
            .UseSessionAffinity()
            .UseLoadBalancing()
            .UsePassiveHealthChecks()
            ;
    });
}

app.Run();
