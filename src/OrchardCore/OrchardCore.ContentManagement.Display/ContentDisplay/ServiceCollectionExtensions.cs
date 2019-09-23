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
        public static ContentPartOptionBuilder AddPartDisplayDriver<TContentPart, TContentPartDisplayDriver>(this IServiceCollection services)
            where TContentPart : ContentPart
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            var builder = new ContentPartOptionBuilder(services, typeof(TContentPart));
            builder.Services.Configure<ContentDisplayOptions>(o => o.TryAddContentPart(builder.ContentPartType));
            builder.WithDisplayDriver<TContentPartDisplayDriver>();
            return builder;
        }

        /// <summary>
        /// Register a display driver for use with a content part.
        /// </summary>
        /// <typeparam name="TContentPartDisplayDriver"></typeparam>
        public static ContentPartOptionBuilder WithDisplayDriver<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            builder.Services.AddScoped<TContentPartDisplayDriver>();
            builder.Services.Configure<ContentDisplayOptions>(o => 
                o.WithPartDisplayDriver(builder.ContentPartType, typeof(TContentPartDisplayDriver)));
            return builder;
        }

        /// <summary>
        /// Add a display driver to a pre-registered content field.
        /// </summary>
        /// <typeparam name="TContentField"></typeparam>
        /// <typeparam name="TContentFieldDisplayDriver"></typeparam>
        public static ContentFieldOptionBuilder AddFieldDisplayDriver<TContentField, TContentFieldDisplayDriver>(this IServiceCollection services)
            where TContentField : ContentField
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            var builder = new ContentFieldOptionBuilder(services, typeof(TContentField));
            builder.Services.Configure<ContentDisplayOptions>(o => o.TryAddContentField(builder.ContentFieldType));
            builder.WithDisplayDriver<TContentFieldDisplayDriver>();
            return builder;
        }

        /// <summary>
        /// Register a display driver for use with a content field.
        /// </summary>
        /// <typeparam name="TContentFieldDisplayDriver"></typeparam>
        /// <param name="builder"></param>
        public static ContentFieldOptionBuilder WithDisplayDriver<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            builder.Services.AddScoped<TContentFieldDisplayDriver>();
            builder.Services.Configure<ContentDisplayOptions>(o =>
                o.WithFieldDisplayDriver(builder.ContentFieldType, typeof(TContentFieldDisplayDriver)));
            return builder;
        }
    }
}
