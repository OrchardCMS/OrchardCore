using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a content part type.
        /// </summary>
        public static IServiceCollection AddContentPart<TContentPart>(this IServiceCollection serviceCollection)
            where TContentPart : ContentPart
        {
            return serviceCollection.Configure<ContentOptions>(o => o.AddContentPart<TContentPart>());
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
