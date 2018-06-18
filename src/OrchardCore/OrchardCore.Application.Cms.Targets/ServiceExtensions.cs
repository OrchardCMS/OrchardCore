namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds Orchard CMS services to the application. 
        /// </summary>
        public static OrchardCoreBuilder AddOrchardCms(this IServiceCollection services)
        {
            return services.AddOrchardCore()

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
        }

        /// <summary>
        /// Adds Orchard CMS services to the application and let the app change the
        /// default tenant behavior and set of features through a configure action.
        /// </summary>
        public static IServiceCollection AddOrchardCms(this IServiceCollection services, System.Action<OrchardCoreBuilder> configure)
        {
            var builder = services.AddOrchardCms();
            configure?.Invoke(builder);
            return services;
        }
    }
}
