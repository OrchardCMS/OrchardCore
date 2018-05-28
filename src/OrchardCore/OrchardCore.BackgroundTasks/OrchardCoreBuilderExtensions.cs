using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level background tasks services.
        /// </summary>
        public static OrchardCoreBuilder AddBackgroundTasks(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices((collection, sp) =>
            {
                collection.AddBackgroundTasks();
            });

            return builder;
        }
    }
}
