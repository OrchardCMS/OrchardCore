using OrchardCore.Clusters;
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup")
    .AddClusteredTenantInactivityCheck()
    ;

builder.Services
    .AddReverseProxy()
    .AddTenantClusters()
    .LoadFromConfig(builder.Configuration.GetSection("OrchardCore_Clusters"))
    ;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseOrchardCore();

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline
        .UseTenantClusters()
        .UseSessionAffinity()
        .UseLoadBalancing()
        .UsePassiveHealthChecks()
        ;
});

app.Run();
