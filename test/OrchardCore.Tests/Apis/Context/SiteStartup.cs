using OrchardCore.Data.YesSql;
using OrchardCore.Modules;
using OrchardCore.Recipes.Services;
using OrchardCore.Testing;
using OrchardCore.Testing.Apis.Security;
using OrchardCore.Testing.Recipes;

namespace OrchardCore.Tests.Apis.Context
{
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
                    "OrchardCore.Apis.GraphQL"
                )
                .ConfigureServices(collection =>
                {
                    collection.Configure<YesSqlOptions>(options =>
                    {
                        // To ensure we don't encounter any concurrent issue, enable EnableThreadSafetyChecks for all test.
                        options.EnableThreadSafetyChecks = true;
                    });

                    collection.AddScoped<IRecipeFileProvider, RecipeFileProvider>();
                    collection.AddScoped<IRecipeHarvester, TestRecipeHarvester>();

                    collection.AddScoped<IAuthorizationHandler, PermissionContextAuthorizationHandler>(sp =>
                    {
                        return new PermissionContextAuthorizationHandler(sp.GetRequiredService<IHttpContextAccessor>(), PermissionsContexts);
                    });
                })
                .Configure(appBuilder => appBuilder.UseAuthorization()));

            services.AddSingleton<IModuleNamesProvider, ModuleNamesProvider>();
        }

#pragma warning disable CA1822 // Mark members as static
        public void Configure(IApplicationBuilder app)
#pragma warning restore CA1822 // Mark members as static
        {
            app.UseOrchardCore();
        }
    }
}
