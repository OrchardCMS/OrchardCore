using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DeferredTasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level deferred tasks services.
        /// </summary>
        public static OrchardCoreBuilder AddDeferredTasks(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.TryAddScoped<IDeferredTaskEngine, DeferredTaskEngine>();
                services.TryAddScoped<IDeferredTaskState, HttpContextTaskState>();
            });

            return builder;
        }
    }
}
