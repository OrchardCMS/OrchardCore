using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a content part type.
        /// </summary>
        public static ContentPartOptionBuilder AddContentPart<TContentPart>(this IServiceCollection services)
            where TContentPart : ContentPart
        {
            services.Configure<ContentOptions>(o => o.AddContentPart<TContentPart>());
            return new ContentPartOptionBuilder(services, typeof(TContentPart));
        }

        /// <summary>
        /// Registers a content field type.
        /// </summary>
        public static IServiceCollection AddContentField<TContentField>(this IServiceCollection serviceCollection)
            where TContentField : ContentField
        {
            return serviceCollection.Configure<ContentOptions>(o => o.AddContentField<TContentField>());
        }
    }
}
