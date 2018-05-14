using OrchardCore.BackgroundTasks;
using OrchardCore.Data;
using OrchardCore.DeferredTasks;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Shell.Data;
using OrchardCore.Mvc;
using OrchardCore.ResourceManagement;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOrchardCms(this IServiceCollection services)
        {
            services
                .AddCommands()

                .AddModules()
                .WithDefaultFeatures("OrchardCore.Settings", "OrchardCore.Setup",
                    "OrchardCore.Recipes", "OrchardCore.Commons")

                .WithMvc()
                .WithAntiForgery()
                .WithAuthentication()
                .WithDataProtection()

                .WithDataAccess()
                .WithTenantDataStorage()
                .WithResourceManagement()
                .WithBackgroundTasks()
                .WithDeferredTasks()
                .WithCaching()

                .WithTheming()
                .WithLiquidViews()
                .WithTagHelpers("OrchardCore.DisplayManagement")
                .WithTagHelpers("OrchardCore.ResourceManagement");

            return services;
        }
    }
}
