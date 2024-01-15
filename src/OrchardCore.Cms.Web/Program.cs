using OrchardCore.Cms.Web;
using OrchardCore.DependencyInjection;
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseOrchardCoreHost()
    .UseNLogHost();

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

builder.Services
    .AddKeyedTransient<IService, FooService>("foo")
    .AddKeyedTransient<IService, BarService>("bar")
    .AddKeyedTransient<IService, BazService>("baz");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseOrchardCore();

app.Run();
