using OrchardCore.Clusters;
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

builder.Services
    .AddReverseProxy()
    .AddClusters()
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
        .UseClusters()
        .UseSessionAffinity()
        .UseLoadBalancing()
        .UsePassiveHealthChecks()
        ;
});

app.Run();
