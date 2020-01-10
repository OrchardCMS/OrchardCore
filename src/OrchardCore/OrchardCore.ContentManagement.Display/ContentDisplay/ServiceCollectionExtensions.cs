using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add a display driver to a pre-registered content part.
        /// By default registers driver for use with all display modes and editors, e.g. *
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
        /// Add a display driver to a pre-registered content part and configure.
        /// </summary>
        /// <typeparam name="TContentPart"></typeparam>
        /// <typeparam name="TContentPartDisplayDriver"></typeparam>
        public static ContentPartOptionBuilder AddPartDisplayDriver<TContentPart, TContentPartDisplayDriver>(this IServiceCollection services, Action<ContentPartDisplayDriverOption> action)
            where TContentPart : ContentPart
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            var builder = new ContentPartOptionBuilder(services, typeof(TContentPart));
            builder.Services.Configure<ContentDisplayOptions>(o => o.TryAddContentPart(builder.ContentPartType));
            builder.WithDisplayDriver<TContentPartDisplayDriver>(action);

            return builder;
        }

        /// <summary>
        /// Register a display driver for use with a content part.
        /// By default registers driver for use with all editors, e.g. *
        /// </summary>
        /// <typeparam name="TContentPartDisplayDriver"></typeparam>
        public static ContentPartOptionBuilder WithDisplayDriver<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            builder.Services.AddScoped<TContentPartDisplayDriver>();
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.TryAddContentPart(builder.ContentPartType);
                o.WithPartDisplayDriver(builder.ContentPartType, typeof(TContentPartDisplayDriver));
            });

            return builder;
        }

        /// <summary>
        /// Register a display driver for use with a content part and configure.
        /// </summary>
        /// <typeparam name="TContentPartDisplayDriver"></typeparam>
        public static ContentPartOptionBuilder WithDisplayDriver<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder, Action<ContentPartDisplayDriverOption> action)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            builder.Services.AddScoped<TContentPartDisplayDriver>();
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.TryAddContentPart(builder.ContentPartType);
                o.WithPartDisplayDriver(builder.ContentPartType, typeof(TContentPartDisplayDriver), action);
            });

            return builder;
        }

        /// <summary>
        /// Add a display driver to a pre-registered content field.
        /// By default registers driver for use with all display modes and editors, e.g. *
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
        /// Add a display driver to a pre-registered content field and configure display modes and editors.
        /// </summary>
        /// <typeparam name="TContentField"></typeparam>
        /// <typeparam name="TContentFieldDisplayDriver"></typeparam>
        public static ContentFieldOptionBuilder AddFieldDisplayDriver<TContentField, TContentFieldDisplayDriver>(this IServiceCollection services, Action<ContentFieldDisplayDriverOption> action)
            where TContentField : ContentField
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            var builder = new ContentFieldOptionBuilder(services, typeof(TContentField));
            builder.Services.Configure<ContentDisplayOptions>(o => o.TryAddContentField(builder.ContentFieldType));
            builder.WithDisplayDriver<TContentFieldDisplayDriver>(action);

            return builder;
        }

        /// <summary>
        /// Register a display driver for use with a content field.
        /// By default registers driver for use with all display modes and editors, e.g. *
        /// </summary>
        /// <typeparam name="TContentFieldDisplayDriver"></typeparam>
        public static ContentFieldOptionBuilder WithDisplayDriver<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            builder.Services.AddScoped<TContentFieldDisplayDriver>();
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.TryAddContentField(builder.ContentFieldType);
                o.WithFieldDisplayDriver(builder.ContentFieldType, typeof(TContentFieldDisplayDriver));
            });

            return builder;
        }

        /// <summary>
        /// Register a display driver for use with a content field and configure display modes and editors.
        /// </summary>
        /// <typeparam name="TContentFieldDisplayDriver"></typeparam>
        public static ContentFieldOptionBuilder WithDisplayDriver<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder, Action<ContentFieldDisplayDriverOption> action)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            builder.Services.AddScoped<TContentFieldDisplayDriver>();
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.TryAddContentField(builder.ContentFieldType);
                o.WithFieldDisplayDriver(builder.ContentFieldType, typeof(TContentFieldDisplayDriver), action);
            });

            return builder;
        }
    }
}
