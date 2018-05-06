using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data;
using OrchardCore.DeferredTasks;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Data;
using OrchardCore.Modules;
using OrchardCore.Mvc;
using OrchardCore.ResourceManagement;
using OrchardCore.ResourceManagement.TagHelpers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOrchardCms(this IServiceCollection services)
        {
            services.AddThemingHost();
            services.AddManifestDefinition("theme");
            services.AddSitesFolder();
            services.AddCommands();
            services.AddAuthentication();

            services.AddModules()

                .WithDefaultFeatures(
                    // Here, we can remove these 2 features
                    //"OrchardCore.Antiforgery", "OrchardCore.Mvc",
                    "OrchardCore.Settings", "OrchardCore.Setup", "OrchardCore.Recipes", "OrchardCore.Commons")

                // Replace "OrchardCore.Commons" but not completely because
                // this module also defines a filter, a controller and a view.
                // Could be replaced by 'AddTenantCommons()'.
                .ConfigureTenantServices(collection =>
                {
                    collection.AddDeferredTasks();
                    collection.AddDataAccess();
                    collection.AddBackgroundTasks();
                    collection.AddResourceManagement();
                    collection.AddCaching();
                    collection.AddShellDescriptorStorage();
                    collection.AddExtensionManager();
                    collection.AddTheming();
                    collection.AddLiquidViews();
                })

                .ConfigureTenant((app, routes, sp) =>
                {
                    sp.AddTagHelpers(typeof(ResourcesTagHelper).Assembly);
                    sp.AddTagHelpers(typeof(ShapeTagHelper).Assembly);
                })

                // Replace "OrchardCore.Mvc"
                // Could be replaced by 'AddTenantMvc()'.
                .ConfigureTenantServices<IServiceProvider>((collection, sp) =>
                {
                    collection.AddMvcModules(sp);
                })

                .ConfigureTenant((app, routes, sp) =>
                 {
                     app.UseStaticFilesModules();
                 })

                // Replace "OrchardCore.DataProtection".
                // Could be replaced by 'AddTenantAddDataProtection()'.
                .ConfigureTenantServices<IOptions<ShellOptions> ,ShellSettings >((collection, options, settings) =>
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
                })

                // Replace "OrchardCore.Antiforgery".
                // Could be replaced by 'AddTenantAntiForgery()'.
                .ConfigureTenantServices<ShellSettings>((collection, settings) =>
                 {
                     var tenantName = settings.Name;
                     var tenantPrefix = "/" + settings.RequestUrlPrefix;

                     collection.AddAntiforgery(options =>
                     {
                         options.Cookie.Name = "orchantiforgery_" + tenantName;
                         options.Cookie.Path = tenantPrefix;
                     });
                 });

            return services;
        }
    }
}
