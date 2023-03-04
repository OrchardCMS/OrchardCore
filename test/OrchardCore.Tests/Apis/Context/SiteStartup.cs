using OrchardCore.Modules;
using OrchardCore.Recipes.Services;
using OrchardCore.Testing;
using OrchardCore.Testing.Apis;
using OrchardCore.Testing.Apis.Security;
using OrchardCore.Testing.Recipes;
using OrchardCore.Testing.Stubs;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms(builder => builder
                .AddSetupFeatures("OrchardCore.Tenants")
                .AddTenantFeatures("OrchardCore.Apis.GraphQL")
                .ConfigureServices(serviceCollection =>
                {
                    serviceCollection.AddScoped<IRecipeFileProvider, RecipeFileProvider>();
                    serviceCollection.AddScoped<IRecipeHarvester, RecipeHarvesterStub>();

                    serviceCollection.AddScoped<IAuthorizationHandler, PermissionContextAuthorizationHandler>(sp =>
                        new PermissionContextAuthorizationHandler(sp.GetRequiredService<IHttpContextAccessor>(),
                        SiteContextOptions.PermissionsContexts));
                })
                .Configure(appBuilder => appBuilder.UseAuthorization()));

            services.AddSingleton<IModuleNamesProvider>(new ModuleNamesProvider(typeof(Program).Assembly));
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerFactory loggerFactory) => app.UseOrchardCore();
    }
}
