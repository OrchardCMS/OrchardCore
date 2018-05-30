namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOrchardCms(this IServiceCollection services)
        {
            return services

                .AddOrchardCore(builder => builder
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
                    .AddCaching());
        }
    }
}
