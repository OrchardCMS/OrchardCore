using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds Orchard CMS services to the application. 
        /// </summary>
        public static IServiceCollection AddOrchardCms(this IServiceCollection services)
        {
            return AddOrchardCms(services, null);
        }

        /// <summary>
        /// Adds Orchard CMS services to the application and let the app change the
        /// default tenant behavior and set of features through a configure action.
        /// </summary>
        public static IServiceCollection AddOrchardCms(this IServiceCollection services, Action<OrchardCoreBuilder> configure)
        {
            var builder = services.AddOrchardCore()

                .AddCommands()

                .AddMvc()

                .AddSetupFeatures("OrchardCore.Setup")

                .AddDataAccess()
                .AddDataStorage()
                .AddBackgroundTasks()
                .AddDeferredTasks()

                .AddTheming()
                .AddLiquidViews()
                .AddResourceManagement()
                .AddGeneratorTagFilter()
                .AddCaching();

            configure?.Invoke(builder);

            return services;
        }
    }
}
