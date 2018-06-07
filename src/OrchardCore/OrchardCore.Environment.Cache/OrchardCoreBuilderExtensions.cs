namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level caching services.
        /// </summary>
        public static OrchardCoreBuilder AddCaching(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices(tenant =>
            {
                tenant.AddCaching();
            });

            return builder;
        }
    }
}
