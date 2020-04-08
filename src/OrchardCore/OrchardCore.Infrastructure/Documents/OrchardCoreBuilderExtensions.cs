using OrchardCore.Data.Documents;
using OrchardCore.Documents;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services to keep in sync any single <see cref="IDocument"/> between an <see cref="IDocumentStore"/> and a multi level cache.
        /// </summary>
        public static OrchardCoreBuilder AddDocumentManagement(this OrchardCoreBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddScoped(typeof(IDocumentManager<>), typeof(DocumentManager<>));
                services.AddScoped(typeof(IVolatileDocumentManager<>), typeof(VolatileDocumentManager<>));
                services.AddSingleton<IDocumentOptionsFactory, DocumentOptionsFactory>();
                services.AddTransient(typeof(DocumentOptions<>));

                services.AddScoped<IPersistentPropertiesService, PersistentPropertiesService>();
                services.AddScoped<IVolatilePropertiesService, VolatilePropertiesService>();
            });
        }
    }
}
