using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add a display driver to a pre-registered content part.
        /// </summary>
        /// <typeparam name="TContentPart"></typeparam>
        /// <typeparam name="TContentPartDisplayDriver"></typeparam>
        /// <param name="services"></param>
        public static ContentPartOptionBuilder AddDisplayDriver<TContentPart, TContentPartDisplayDriver>(this IServiceCollection services)
            where TContentPart : ContentPart
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            services.AddScoped<TContentPartDisplayDriver>();

            var builder = new ContentPartOptionBuilder(services, typeof(TContentPart));
            builder.WithDisplayDriver<TContentPartDisplayDriver>();
            return builder;
        }

        /// <summary>
        /// Register a display driver for use with a content part.
        /// </summary>
        /// <typeparam name="TContentPartDisplayDriver"></typeparam>
        /// <param name="contentPartBuilder"></param>
        public static ContentPartOptionBuilder WithDisplayDriver<TContentPartDisplayDriver>(this ContentPartOptionBuilder contentPartBuilder)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            contentPartBuilder.Services.AddScoped<TContentPartDisplayDriver>();
            contentPartBuilder.Services.Configure<ContentOptions>(o =>
                o.WithContentPartResolver(typeof(IContentPartDisplayDriver), contentPartBuilder.ContentPartType, typeof(TContentPartDisplayDriver)));
            return contentPartBuilder;
        }
    }
}
