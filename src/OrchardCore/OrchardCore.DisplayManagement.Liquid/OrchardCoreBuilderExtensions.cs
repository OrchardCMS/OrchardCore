namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing liquid view template files.
        /// </summary>
        public static OrchardCoreBuilder AddLiquidViews(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices(tenant =>
            {
                tenant.AddLiquidViews();
            });

            return builder;
        }
    }
}