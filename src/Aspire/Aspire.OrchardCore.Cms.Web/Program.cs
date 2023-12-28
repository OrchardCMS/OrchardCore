using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();
builder.AddServiceDefaults();

var ocBuilder = builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

if (builder.Configuration.GetValue<bool>("EnableAzureCoreFeatures"))
{
    // Enable Azure based resources to store the shell configs and the data protections.
    ocBuilder.AddAzureShellsConfiguration()
        .AddGlobalFeatures("OrchardCore.DataProtection.Azure");
}

if (builder.Configuration.GetValue<bool>("EnableRedisCache"))
{
    ocBuilder.AddSetupFeatures("OrchardCore.Redis", "OrchardCore.Redis.Lock")
        .AddGlobalFeatures("OrchardCore.Redis", "OrchardCore.Redis.Cache", "OrchardCore.Redis.DataProtection", "OrchardCore.Redis.Lock");
}

if (builder.Configuration.GetValue<bool>("EnableAzureStorage"))
{
    ocBuilder.AddTenantFeatures("OrchardCore.Media.Azure.Storage");
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseOrchardCore();

await app.RunAsync();
