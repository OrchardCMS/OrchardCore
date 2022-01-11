#if (UseNLog)
using OrchardCore.Logging;
#endif
#if (UseSerilog)
using Serilog;
#endif

var builder = WebApplication.CreateBuilder(args);

#if (UseNLog)
builder.Host.UseNLogWeb();
#endif
#if (UseSerilog)
builder.Host.UseSerilog((hostingContext, configBuilder) =>
                {
                    configBuilder.ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext();
                });
#endif

builder.Services.AddOrchardCms();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

#if (UseSerilog)
app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());
#else
app.UseOrchardCore();
#endif

app.Run();
