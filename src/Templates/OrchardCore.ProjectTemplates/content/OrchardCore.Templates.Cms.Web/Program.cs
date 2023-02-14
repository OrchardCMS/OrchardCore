#if (UseNLog)
using OrchardCore.Logging;
#endif
#if (UseSerilog)
using Serilog;
#endif

var builder = WebApplication.CreateBuilder(args);

#if (UseNLog)
builder.Host.UseNLogHost();
#endif
#if (UseSerilog)
builder.Host.UseSerilog((hostingContext, configBuilder) =>
    {
        configBuilder.ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.FromLogContext();
    });
#endif

builder.Services
    .AddOrchardCms()
    // // Orchard Specific Pipeline
    // .ConfigureServices( services => {
    // })
    // .Configure( (app, routes, services) => {
    // })
;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

#if (UseSerilog)
app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());
#else
app.UseOrchardCore();
#endif

app.Run();
