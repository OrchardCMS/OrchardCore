using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantServiceCollectionExtensions
    {
        public static IServiceCollection WithAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication();

            return services.ConfigureTenantServices<ShellSettings>((collection, settings) =>
            {
                collection.AddAuthentication();

                // Note: IAuthenticationSchemeProvider is already registered at the host level.
                // We need to register it again so it is taken into account at the tenant level.
                collection.AddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
            })

            .ConfigureTenant((app, routes, sp) =>
            {
                app.UseAuthentication();
            });
        }

        public static IServiceCollection WithAntiForgery(this IServiceCollection services)
        {
            return services.ConfigureTenantServices<ShellSettings>((collection, settings) =>
            {
                var tenantName = settings.Name;
                var tenantPrefix = "/" + settings.RequestUrlPrefix;

                collection.AddAntiforgery(options =>
                {
                    options.Cookie.Name = "orchantiforgery_" + tenantName;
                    options.Cookie.Path = tenantPrefix;
                });
            });
        }

        public static IServiceCollection WithDataProtection(this IServiceCollection services)
        {
            return services.PostConfigureTenantServices<IOptions<ShellOptions>, ShellSettings>((collection, options, settings) =>
            {
                var directory = Directory.CreateDirectory(Path.Combine(
                options.Value.ShellsApplicationDataPath,
                options.Value.ShellsContainerName,
                settings.Name, "DataProtection-Keys"));

                // Re-register the data protection services to be tenant-aware so that modules that internally
                // rely on IDataProtector/IDataProtectionProvider automatically get an isolated instance that
                // manages its own key ring and doesn't allow decrypting payloads encrypted by another tenant.
                // By default, the key ring is stored in the tenant directory of the configured App_Data path.
                collection.Add(new ServiceCollection()
                    .AddDataProtection()
                    .PersistKeysToFileSystem(directory)
                    .SetApplicationName(settings.Name)
                    .Services);
            });
        }
    }
}
