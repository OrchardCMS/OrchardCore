using OrchardCore.Modules;

namespace OrchardCore.DeferredTasks
{
    public static class ModularServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level deferred tasks services.
        /// </summary>
        public static ModularServicesBuilder AddDeferredTasks(this ModularServicesBuilder builder)
        {
            builder.Services.ConfigureTenantServices((collection) =>
            {
                collection.AddDeferredTasks();
            });

            return builder;
        }
    }
}
