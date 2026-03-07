using Fluid;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Liquid;
using OrchardCore.ContentManagement.Display.Placement;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Liquid;

namespace Microsoft.Extensions.DependencyInjection;

public static class ContentManagementDisplayServiceCollectionExtensions
{
    public static IServiceCollection AddContentManagementDisplay(this IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<ContentItemViewModel>();
            o.MemberAccessStrategy.Register<ContentPartViewModel>();
        });

        services.TryAddTransient<IContentItemDisplayManager, ContentItemDisplayManager>();

        services.AddScoped<IContentDisplayHandler, ContentItemDisplayCoordinator>();

        services.AddScoped<IPlacementNodeFilterProvider, ContentTypePlacementNodeFilterProvider>();
        services.AddScoped<IPlacementNodeFilterProvider, ContentPartPlacementNodeFilterProvider>();

        services.AddScoped<IContentPartDisplayDriverResolver, ContentPartDisplayDriverResolver>();
        services.AddScoped<IContentFieldDisplayDriverResolver, ContentFieldDisplayDriverResolver>();

        services.AddOptions<ContentDisplayOptions>();

        services.AddLiquidFilter<ConsoleLogFilter>("console_log");

        return services;
    }
}
