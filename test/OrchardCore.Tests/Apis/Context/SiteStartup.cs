using OrchardCore.Modules;
using OrchardCore.Modules.Manifest;
using OrchardCore.Recipes.Services;

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

        private class ModuleNamesProvider : IModuleNamesProvider
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
}
