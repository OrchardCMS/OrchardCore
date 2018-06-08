namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds Orchard CMS services to the application. 
        /// </summary>
        public static IServiceCollection AddOrchardCms(this IServiceCollection services)
        {
            return services.AddOrchardCore()

                .AddCommands()

                .AddMvc()
                .AddAntiForgery()
                .AddAuthentication()
                .AddDataProtection()

                .WithDefaultFeatures("OrchardCore.Setup")

                .AddDataAccess()
                .AddDataStorage()
                .AddBackgroundTasks()
                .AddDeferredTasks()

                .AddTheming()
                .AddLiquidViews()
                .AddResourceManagement()
                .AddGeneratorTagFilter()
                .AddCaching()

                .Services;
        }
    }
}
