using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Documents;
using OrchardCore.Documents;
using OrchardCore.Documents.Options;

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
                services.AddSingleton(typeof(IDocumentManager<>), typeof(DocumentManager<>));
                services.AddSingleton(typeof(IVolatileDocumentManager<>), typeof(VolatileDocumentManager<>));
                services.AddSingleton(typeof(IDocumentManager<,>), typeof(DocumentManager<,>));

                services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<DocumentOptions>, DocumentOptionsSetup>());

                services.AddSingleton(typeof(IDocumentEntityManager<>), typeof(DocumentEntityManager<>));
                services.AddSingleton(typeof(IVolatileDocumentEntityManager<>), typeof(VolatileDocumentEntityManager<>));
                services.AddSingleton(typeof(IDocumentEntityManager<,>), typeof(DocumentEntityManager<,>));
            });
        }
    }
}
