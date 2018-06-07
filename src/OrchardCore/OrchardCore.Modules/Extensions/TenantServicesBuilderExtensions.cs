using System;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantServicesBuilderExtensions
    {
        /// <summary>
        /// Registers at the tenant level a set of features which are always enabled for this tenant.
        /// </summary>
        public static TenantServicesBuilder AddEnabledFeatures(this TenantServicesBuilder tenant, params string[] featureIds)
        {
            foreach (var featureId in featureIds)
            {
                tenant.Services.AddTransient(sp => new ShellFeature(featureId, alwaysEnabled: true));
            }

            return tenant;
        }

        /// <summary>
        /// Adds tenant level antiforgery services.
        /// </summary>
        public static TenantServicesBuilder AddAntiForgery(this TenantServicesBuilder tenant)
        {
            var settings = tenant.ServiceProvider.GetRequiredService<ShellSettings>();

            var tenantName = settings.Name;
            var tenantPrefix = "/" + settings.RequestUrlPrefix;

            tenant.Services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "orchantiforgery_" + tenantName;
                options.Cookie.Path = tenantPrefix;
            });

            return tenant;
        }

        /// <summary>
        /// Adds tenant level authentication services. Note: the related middleware needs to be added.
        /// </summary>
        public static TenantServicesBuilder AddAuthentication(this TenantServicesBuilder tenant)
        {
            tenant.Services.AddAuthentication();

            // Note: IAuthenticationSchemeProvider is already registered at the host level.
            // We need to register it again so it is taken into account at the tenant level.
            tenant.Services.AddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();

            return tenant;
        }

        /// <summary>
        /// Adds tenant level data protection services.
        /// </summary>
        public static TenantServicesBuilder AddDataProtection(this TenantServicesBuilder tenant)
        {
            var settings = tenant.ServiceProvider.GetRequiredService<ShellSettings>();
            var options = tenant.ServiceProvider.GetRequiredService<IOptions<ShellOptions>>();

            var directory = Directory.CreateDirectory(Path.Combine(
            options.Value.ShellsApplicationDataPath,
            options.Value.ShellsContainerName,
            settings.Name, "DataProtection-Keys"));

            // Re-register the data protection services to be tenant-aware so that modules that internally
            // rely on IDataProtector/IDataProtectionProvider automatically get an isolated instance that
            // manages its own key ring and doesn't allow decrypting payloads encrypted by another tenant.
            // By default, the key ring is stored in the tenant directory of the configured App_Data path.
            tenant.Services.Add(new ServiceCollection()
                .AddDataProtection()
                .PersistKeysToFileSystem(directory)
                .SetApplicationName(settings.Name)
                .Services);

            return tenant;
        }
    }
}
