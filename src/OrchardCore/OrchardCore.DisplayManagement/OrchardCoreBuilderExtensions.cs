using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Events;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.LocationExpander;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Mvc.LocationExpander;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Adds host and tenant level services for managing themes.
    /// </summary>
    public static OrchardCoreBuilder AddTheming(this OrchardCoreBuilder builder)
    {
        builder.AddThemingHost()
            .ConfigureServices(services =>
            {
                services.Configure<MvcOptions>((options) =>
                {
                    options.Filters.Add<ModelBinderAccessorFilter>();
                    options.Filters.Add<NotifyFilter>();
                    options.Filters.Add<RazorViewActionFilter>();
                });

                services.AddTransient<IConfigureOptions<NotifyJsonSerializerOptions>, NotifyJsonSerializerOptionsConfiguration>();

                // Used as a service when we create a fake 'ActionContext'.
                services.AddScoped<IAsyncViewActionFilter, RazorViewActionFilter>();

                services.AddScoped<IUpdateModelAccessor, LocalModelBinderAccessor>();
                services.AddScoped<ViewContextAccessor>();

                services.AddScoped<RazorShapeTemplateViewEngine>();
                services.AddScoped<IShapeTemplateViewEngine>(sp => sp.GetService<RazorShapeTemplateViewEngine>());

                services.AddSingleton<IApplicationFeatureProvider<ViewsFeature>, ThemingViewsFeatureProvider>();
                services.AddScoped<IViewLocationExpanderProvider, ThemeViewLocationExpanderProvider>();

                services.AddScoped<IShapeTemplateHarvester, BasicShapeTemplateHarvester>();
                services.AddKeyedSingleton<IDictionary<string, ShapeTable>>(nameof(DefaultShapeTableManager), new ConcurrentDictionary<string, ShapeTable>());
                services.AddScoped<IShapeTableManager, DefaultShapeTableManager>();

                services.AddShapeTableProvider<ShapeAttributeBindingStrategy>();
                services.AddShapeTableProvider<ShapePlacementParsingStrategy>();
                services.AddShapeTableProvider<ShapeTemplateBindingStrategy>();

                services.AddScoped<IPlacementNodeFilterProvider, PathPlacementNodeFilterProvider>();

                services.AddScoped<IShapePlacementProvider, ShapeTablePlacementProvider>();

                services.AddTransient<IConfigureOptions<ShapeTemplateOptions>, ShapeTemplateOptionsSetup>();
                services.TryAddSingleton<IShapeTemplateFileProviderAccessor, ShapeTemplateFileProviderAccessor>();

                services.AddShapeAttributes<CoreShapes>();
                services.AddShapeTableProvider<CoreShapesTableProvider>();
                services.AddShapeAttributes<ZoneShapes>();
                services.AddShapeTableProvider<ZoneShapeAlternates>();
                services.AddShapeAttributes<GroupShapes>();

                services.AddScoped(typeof(IDisplayManager<>), typeof(DisplayManager<>));
                services.AddScoped<IHtmlDisplay, DefaultHtmlDisplay>();
                services.AddScoped<ILayoutAccessor, LayoutAccessor>();
                services.AddScoped<IThemeManager, ThemeManager>();
                services.AddScoped<IPageTitleBuilder, PageTitleBuilder>();

                services.AddScoped<IShapeFactory, DefaultShapeFactory>();
                services.AddScoped<IDisplayHelper, DisplayHelper>();

                services.AddScoped<INotifier, Notifier>();

                services.AddShapeAttributes<DateTimeShapes>();
                services.AddShapeAttributes<PageTitleShapes>();

                services.AddTagHelpers<AddAlternateTagHelper>();
                services.AddTagHelpers<AddClassTagHelper>();
                services.AddTagHelpers<AddWrapperTagHelper>();
                services.AddTagHelpers<ClearAlternatesTagHelper>();
                services.AddTagHelpers<ClearClassesTagHelper>();
                services.AddTagHelpers<ClearWrappersTagHelper>();
                services.AddTagHelpers<DateTimeTagHelper>();
                services.AddTagHelpers<InputIsDisabledTagHelper>();
                services.AddTagHelpers<RemoveAlternateTagHelper>();
                services.AddTagHelpers<RemoveClassTagHelper>();
                services.AddTagHelpers<RemoveWrapperTagHelper>();
                services.AddTagHelpers<ShapeMetadataTagHelper>();
                services.AddTagHelpers<ShapeTagHelper>();
                services.AddTagHelpers<TimeSpanTagHelper>();
                services.AddTagHelpers<ValidationMessageTagHelper>();
                services.AddTagHelpers<ZoneTagHelper>();
            });

        return builder;
    }

    /// <summary>
    /// Adds host level services for managing themes.
    /// </summary>
    public static OrchardCoreBuilder AddThemingHost(this OrchardCoreBuilder builder)
    {
        var services = builder.ApplicationServices;

        services.AddTransient<IExtensionDependencyStrategy, ThemeExtensionDependencyStrategy>();
        services.AddTransient<IFeatureBuilderEvents, ThemeFeatureBuilderEvents>();

        return builder;
    }
}
