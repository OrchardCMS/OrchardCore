using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Modules.LocationExpander;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Events;
using Orchard.DisplayManagement.Extensions;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.LocationExpander;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.DisplayManagement.Razor;
using Orchard.DisplayManagement.Shapes;
using Orchard.DisplayManagement.Theming;
using Orchard.DisplayManagement.Title;
using Orchard.DisplayManagement.Zones;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;

namespace Orchard.DisplayManagement
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds host level services.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddThemingHost(this IServiceCollection services)
        {
		    services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(ModelBinderAccessorFilter));
            });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(new ThemingFileProvider());
            });
			
            services.AddScoped<IUpdateModelAccessor, LocalModelBinderAccessor>();
            services.AddScoped<IViewLocationExpanderProvider, ThemeAwareViewLocationExpanderProvider>();

            services.AddSingleton<IExtensionDependencyStrategy, ThemeExtensionDependencyStrategy>();
            services.AddSingleton<IShapeTemplateViewEngine, RazorShapeTemplateViewEngine>();

            services.AddSingleton<IFeatureBuilderEvents, ThemeFeatureBuilderEvents>();

            return services;
        }

        public static IServiceCollection AddTheming(this IServiceCollection services)
        {
		    services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(NotifyFilter));
            });
		
            services.AddScoped<IShapeTemplateHarvester, BasicShapeTemplateHarvester>();
            services.AddTransient<IShapeTableManager, DefaultShapeTableManager>();

            services.AddScoped<IShapeTableProvider, ShapeAttributeBindingStrategy>();
            services.AddScoped<IShapeTableProvider, ShapePlacementParsingStrategy>();
            services.AddScoped<IShapeTableProvider, ShapeTemplateBindingStrategy>();

            services.AddShapeAttributes<CoreShapes>();
            services.AddScoped<IShapeTableProvider, CoreShapesTableProvider>();
            services.AddShapeAttributes<ZoneShapes>();
            services.AddScoped<IShapeTableProvider, LayoutShapes>();

            services.AddScoped<IHtmlDisplay, DefaultHtmlDisplay>();
            services.AddScoped<ILayoutAccessor, LayoutAccessor>();
            services.AddScoped<IThemeManager, ThemeManager>();
            services.AddScoped<IPageTitleBuilder, PageTitleBuilder>();

            services.AddScoped<IShapeFactory, DefaultShapeFactory>();
            services.AddScoped<IDisplayHelperFactory, DisplayHelperFactory>();

            services.AddScoped<INotifier, Notifier>();

            services.AddScoped(typeof(IPluralStringLocalizer<>), typeof(PluralStringLocalizer<>));
            services.AddShapeAttributes<DateTimeShapes>();

            return services;
        }
    }
}
