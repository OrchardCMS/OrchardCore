using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;

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
        public static ContentFieldOptionBuilder AddContentField<TContentField>(this IServiceCollection services)
            where TContentField : ContentField
        {
            services.Configure<ContentOptions>(o => o.AddContentField<TContentField>());
            return new ContentFieldOptionBuilder(services, typeof(TContentField));
        }

        /// <summary>
        /// Register a handler for use with a content part.
        /// </summary>
        /// <typeparam name="TContentPartHandler"></typeparam>
        public static ContentPartOptionBuilder WithHandler<TContentPartHandler>(this ContentPartOptionBuilder builder)
            where TContentPartHandler : class, IContentPartHandler
        {
            builder.Services.AddScoped<TContentPartHandler>();
            builder.Services.Configure<ContentOptions>(o => {
                o.WithPartHandler(builder.ContentPartType, typeof(TContentPartHandler));
            });
            return builder;
        }

        /// <summary>
        /// Add a handler to a pre-registered content part.
        /// </summary>
        /// <typeparam name="TContentPart"></typeparam>
        /// <typeparam name="TContentPartHandler"></typeparam>
        public static ContentPartOptionBuilder AddPartHandler<TContentPart, TContentPartHandler>(this IServiceCollection services)
            where TContentPart : ContentPart
            where TContentPartHandler : class, IContentPartHandler
        {
            var builder = new ContentPartOptionBuilder(services, typeof(TContentPart));
            builder.Services.Configure<ContentOptions>(o => o.TryAddContentPart(builder.ContentPartType));
            builder.WithHandler<TContentPartHandler>();
            return builder;
        }
    }
}
