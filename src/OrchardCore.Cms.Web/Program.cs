using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogWeb();

builder.Services.AddOrchardCms().AddSetupFeatures("OrchardCore.AutoSetup");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseOrchardCore();

app.Run();
