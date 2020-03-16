using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Documents
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services to keep in sync the <see cref="Data.Documents.IDocumentStore"/> with a multi level distributed cache.
        /// </summary>
        public static IServiceCollection AddDocumentCaching(this IServiceCollection services)
        {
            services.TryAddScoped(typeof(IDocumentManager<>), typeof(DocumentManager<>));
            services.TryAddSingleton<IDocumentOptionsFactory, DocumentOptionsFactory>();
            services.TryAddTransient(typeof(DocumentOptions<>));
            return services;
        }
    }
}
