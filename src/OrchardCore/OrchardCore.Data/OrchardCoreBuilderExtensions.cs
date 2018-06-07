namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level data access services.
        /// </summary>
        public static OrchardCoreBuilder AddDataAccess(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices(tenant =>
            {
                tenant.AddDataAccess();
            });

            return builder;
        }
    }
}