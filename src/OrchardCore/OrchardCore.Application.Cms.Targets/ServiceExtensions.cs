namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds Orchard CMS services to the application. 
        /// </summary>
        public static IServiceCollection AddOrchardCms(this IServiceCollection services)
        {
            services.AddOrchardCore()

                .AddCommands()

                .AddMvc()
                .AddAntiForgery()
                .AddAuthentication()
                .AddDataProtection()

                .AddSetupFeatures(
					"OrchardCore.Setup",
					"OrchardCore.Apis.GraphQL",
                    "OrchardCore.Apis.JsonApi",
                    "OrchardCore.Apis.OpenApi")

                .AddDataAccess()
                .AddDataStorage()
                .AddBackgroundTasks()
                .AddDeferredTasks()

                .AddTheming()
                .AddLiquidViews()
                .AddResourceManagement()
                .AddGeneratorTagFilter()
                .AddCaching();


            return services;
        }
    }
}
