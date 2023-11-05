using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer;

public static class ContentHandlerOptionsExtensions
{
    public static IServiceCollection AddContentPartImportHandler<TPart, THandler>(this IServiceCollection services)
        where TPart : ContentPart
        where THandler : IContentPartImportHandler
    {
        services.AddScoped(typeof(THandler));
        services.Configure<ContentHandlerOptions>(options =>
        {
            options.AddPartHandler(typeof(TPart), typeof(THandler));
        });

        return services;
    }

    public static IServiceCollection AddContentFieldImportHandler<TField, THandler>(this IServiceCollection services)
        where TField : ContentField
        where THandler : IContentFieldImportHandler
    {
        services.AddScoped(typeof(THandler));
        services.Configure<ContentHandlerOptions>(options =>
        {
            options.AddFieldHandler(typeof(TField), typeof(THandler));
        });

        return services;
    }
}
