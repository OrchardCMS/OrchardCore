namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level background tasks services.
        /// </summary>
        public static OrchardCoreBuilder AddBackgroundTasks(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices(tenant =>
            {
                tenant.AddBackgroundTasks();
            });

            return builder;
        }
    }
}
