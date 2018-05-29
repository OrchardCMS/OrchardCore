namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level deferred tasks services.
        /// </summary>
        public static OrchardCoreBuilder AddDeferredTasks(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices((tenant, sp) =>
            {
                tenant.AddDeferredTasks();
            });

            return builder;
        }
    }
}
