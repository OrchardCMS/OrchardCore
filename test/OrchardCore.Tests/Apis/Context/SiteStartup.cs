using OrchardCore.Data.YesSql;
using OrchardCore.Modules;
using OrchardCore.Modules.Manifest;
using OrchardCore.Recipes.Services;
using OrchardCore.Tests.Modules.OrchardCore.Jobs;

namespace OrchardCore.Tests.Apis.Context;

public class SiteStartup
{
    public static readonly ConcurrentDictionary<string, PermissionsContext> PermissionsContexts;

    static SiteStartup()
    {
        PermissionsContexts = new ConcurrentDictionary<string, PermissionsContext>();
    }

#pragma warning disable CA1822 // Mark members as static
    public void ConfigureServices(IServiceCollection services)
#pragma warning restore CA1822 // Mark members as static
    {
        services.AddOrchardCms(builder =>
            builder.AddSetupFeatures(
                "OrchardCore.Tenants"
            )
            .AddTenantFeatures(
                "OrchardCore.Localization",
                "OrchardCore.Apis.GraphQL"
            )
            .ConfigureServices(collection =>
            {
                collection.Configure<YesSqlOptions>(options =>
                {
                    // To ensure we don't encounter any concurrent issue, enable EnableThreadSafetyChecks for all test.
                    options.EnableThreadSafetyChecks = true;
                });

                collection.AddScoped<IRecipeHarvester, TestRecipeHarvester>();

                // The jobs of the tests of the jobs engine, which run on the tenants that enable the feature.
                collection.AddJob<TestJob>(TestJob.Name, job => job.RetryDelay = TimeSpan.FromMilliseconds(100));
                collection.AddJob<TestJob>(TestJob.DeclaredScheduleName, job => job.Schedule = "0 0 1 1 *");
                collection.AddJob<TestJob>(TestJob.TransientName, job => job.KeepSucceededRuns = false);
                collection.AddJob<TestJob>(TestJob.ManualName, job => job.AllowManualRun = true);

                collection.AddScoped<IAuthorizationHandler, PermissionContextAuthorizationHandler>(sp =>
                {
                    return new PermissionContextAuthorizationHandler(sp.GetRequiredService<IHttpContextAccessor>(), PermissionsContexts);
                });
            })
            .Configure(appBuilder =>
            {
                appBuilder.UseAuthorization();

                // Regression probe for the duplicate endpoint name crash. Runs inside the tenant
                // pipeline (after UseRouting) where the endpoint data source is fully populated, so
                // link generation by name exercises the same code path that used to throw an
                // 'InvalidOperationException' about duplicate endpoint names.
                appBuilder.Use(async (context, next) =>
                {
                    if (context.Request.Path == "/__eptnametest")
                    {
                        var endpointName = context.Request.Query["name"].ToString();
                        var linkGenerator = context.RequestServices.GetRequiredService<global::Microsoft.AspNetCore.Routing.LinkGenerator>();

                        // Throws 'InvalidOperationException' when duplicate non-suppressed named
                        // endpoints exist (the bug being guarded against).
                        var path = linkGenerator.GetPathByName(context, endpointName, new { name = "test" });

                        await context.Response.WriteAsync(path ?? string.Empty);

                        return;
                    }

                    await next();
                });
            }));

        services.AddSingleton<IModuleNamesProvider, ModuleNamesProvider>();
    }

#pragma warning disable CA1822 // Mark members as static
    public void Configure(IApplicationBuilder app)
#pragma warning restore CA1822 // Mark members as static
    {
        app.UseOrchardCore();
    }

    private sealed class ModuleNamesProvider : IModuleNamesProvider
    {
        private readonly string[] _moduleNames;

        public ModuleNamesProvider()
        {
            var assembly = Assembly.Load(new AssemblyName(typeof(Program).Assembly.GetName().Name));
            _moduleNames = assembly.GetCustomAttributes<ModuleNameAttribute>().Select(m => m.Name).ToArray();
        }

        public IEnumerable<string> GetModuleNames()
        {
            return _moduleNames;
        }
    }
}
