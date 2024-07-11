using Microsoft.OpenApi.Models;
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms(orchardCoreBuilder =>
    {
        orchardCoreBuilder.ApplicationServices.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        });
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseOrchardCore();

await app.RunAsync();
