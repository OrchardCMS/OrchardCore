using OrchardCore.Modules;

namespace OrchardCore.DeferredTasks
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level deferred tasks services.
        /// </summary>
        public static OrchardCoreBuilder AddDeferredTasks(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices((collection, sp) =>
            {
                collection.AddDeferredTasks();
            });

            return builder;
        }
    }
}
