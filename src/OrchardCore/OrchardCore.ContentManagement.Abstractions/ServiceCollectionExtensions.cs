using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentManagement;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a content part type. This method may be called multiple times safely,
    /// to reconfigure an existing part.
    /// </summary>
    public static ContentPartOptionBuilder AddContentPart<TContentPart>(this IServiceCollection services)
        where TContentPart : ContentPart
    {
        return services.AddContentPart(typeof(TContentPart));
    }

    /// <summary>
    /// Registers a content part type. This method may be called multiple times safely,
    /// to reconfigure an existing part.
    /// </summary>
    public static ContentPartOptionBuilder AddContentPart(this IServiceCollection services, Type contentPartType)
    {
        services.Configure<ContentOptions>(o => o.GetOrAddContentPart(contentPartType));
        return new ContentPartOptionBuilder(services, contentPartType);
    }

    /// <summary>
    /// Registers a content field type. This method may be called multiple times safely,
    /// to reconfigure an existing field.
    /// </summary>
    public static ContentFieldOptionBuilder AddContentField<TContentField>(this IServiceCollection services)
        where TContentField : ContentField
    {
        return services.AddContentField(typeof(TContentField));
    }

    /// <summary>
    /// Registers a content field type. This method may be called multiple times safely,
    /// to reconfigure an existing field.
    /// </summary>
    public static ContentFieldOptionBuilder AddContentField(this IServiceCollection services, Type contentFieldType)
    {
        services.Configure<ContentOptions>(o => o.GetOrAddContentField(contentFieldType));
        return new ContentFieldOptionBuilder(services, contentFieldType);
    }

    /// <summary>
    /// Register a handler for use with a content part.
    /// </summary>
    /// <typeparam name="TContentPartHandler"></typeparam>
    public static ContentPartOptionBuilder AddHandler<TContentPartHandler>(this ContentPartOptionBuilder builder)
        where TContentPartHandler : class, IContentPartHandler
    {
        return builder.AddHandler(typeof(TContentPartHandler));
    }

    /// <summary>
    /// Register a handler for use with a content part.
    /// </summary>
    public static ContentPartOptionBuilder AddHandler(this ContentPartOptionBuilder builder, Type handlerType)
    {
        builder.Services.TryAddScoped(handlerType);
        builder.Services.Configure<ContentOptions>(o =>
        {
            o.AddPartHandler(builder.ContentPartType, handlerType);
        });

        return builder;
    }

    /// <summary>
    /// Remove a handler registration from a content part.
    /// </summary>
    /// <typeparam name="TContentPartHandler"></typeparam>
    public static ContentPartOptionBuilder RemoveHandler<TContentPartHandler>(this ContentPartOptionBuilder builder)
        where TContentPartHandler : class, IContentPartHandler
    {
        return builder.RemoveHandler(typeof(TContentPartHandler));
    }

    /// <summary>
    /// Remove a handler registration from a content part.
    /// </summary>
    public static ContentPartOptionBuilder RemoveHandler(this ContentPartOptionBuilder builder, Type handlerType)
    {
        builder.Services.RemoveAll(handlerType);
        builder.Services.Configure<ContentOptions>(o =>
        {
            o.RemovePartHandler(builder.ContentPartType, handlerType);
        });

        return builder;
    }

    /// <summary>
    /// Register a handler for use with a content field.
    /// </summary>
    /// <typeparam name="TContentFieldHandler"></typeparam>
    public static ContentFieldOptionBuilder AddHandler<TContentFieldHandler>(this ContentFieldOptionBuilder builder)
        where TContentFieldHandler : class, IContentFieldHandler
    {
        return builder.AddHandler(typeof(TContentFieldHandler));
    }

    /// <summary>
    /// Register a handler for use with a content field.
    /// </summary>
    public static ContentFieldOptionBuilder AddHandler(this ContentFieldOptionBuilder builder, Type handlerType)
    {
        builder.Services.TryAddScoped(handlerType);
        builder.Services.Configure<ContentOptions>(o =>
        {
            o.AddFieldHandler(builder.ContentFieldType, handlerType);
        });

        return builder;
    }

    /// <summary>
    /// Remove a handler registration from a content field.
    /// </summary>
    /// <typeparam name="TContentFieldHandler"></typeparam>
    public static ContentFieldOptionBuilder RemoveHandler<TContentFieldHandler>(this ContentFieldOptionBuilder builder)
        where TContentFieldHandler : class, IContentFieldHandler
    {
        return builder.RemoveHandler(typeof(TContentFieldHandler));
    }

    /// <summary>
    /// Remove a handler registration from a content field.
    /// </summary>
    public static ContentFieldOptionBuilder RemoveHandler(this ContentFieldOptionBuilder builder, Type handlerType)
    {
        builder.Services.RemoveAll(handlerType);
        builder.Services.Configure<ContentOptions>(o =>
        {
            o.RemoveFieldHandler(builder.ContentFieldType, handlerType);
        });

        return builder;
    }

    /// <summary>
    /// Registers a content part schema handler that provides JSON Schema for part settings.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddContentPartSchemaHandler<THandler>(this IServiceCollection services)
        where THandler : class, IContentPartSchemaHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IContentPartSchemaHandler, THandler>());
        return services;
    }

    /// <summary>
    /// Registers a content field schema handler that provides JSON Schema for field settings.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddContentFieldSchemaHandler<THandler>(this IServiceCollection services)
        where THandler : class, IContentFieldSchemaHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IContentFieldSchemaHandler, THandler>());
        return services;
    }


    /// <summary>
    /// Registers a content part schema handler for the part being configured.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="builder">The content part option builder.</param>
    public static ContentPartOptionBuilder WithSchemaHandler<THandler>(this ContentPartOptionBuilder builder)
        where THandler : class, IContentPartSchemaHandler
    {
        builder.Services.AddContentPartSchemaHandler<THandler>();
        return builder;
    }

    /// <summary>
    /// Registers a content field schema handler for the field being configured.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="builder">The content field option builder.</param>
    public static ContentFieldOptionBuilder WithSchemaHandler<THandler>(this ContentFieldOptionBuilder builder)
        where THandler : class, IContentFieldSchemaHandler
    {
        builder.Services.AddContentFieldSchemaHandler<THandler>();
        return builder;
    }
}
