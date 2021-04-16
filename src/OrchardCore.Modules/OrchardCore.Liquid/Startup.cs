using System;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.Indexing;
using OrchardCore.Liquid.Drivers;
using OrchardCore.Liquid.Filters;
using OrchardCore.Liquid.Handlers;
using OrchardCore.Liquid.Indexing;
using OrchardCore.Liquid.Models;
using OrchardCore.Liquid.Services;
using OrchardCore.Modules;

namespace OrchardCore.Liquid
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISlugService, SlugService>();
            services.AddScoped<ILiquidTemplateManager, LiquidTemplateManager>();

            services.Configure<TemplateOptions>(options =>
            {
                options.Filters.AddFilter("t", LiquidViewFilters.Localize);
                options.Filters.AddFilter("html_class", LiquidViewFilters.HtmlClass);
                options.Filters.AddFilter("shape_properties", LiquidViewFilters.ShapeProperties);

                // Used to provide a factory to return a value based on a property name that is unknown at registration time.
                options.MemberAccessStrategy.Register<LiquidPropertyAccessor, FluidValue>((obj, name) => obj.GetValueAsync(name));

                // When a property of a JObject value is accessed, try to look into its properties
                options.MemberAccessStrategy.Register<JObject, object>((source, name) => source[name]);

                // Convert JToken to FluidValue
                options.ValueConverters.Add(x => x is JObject o ? new ObjectValue(o) : null);
                options.ValueConverters.Add(x => x is JValue v ? v.Value : null);
                options.ValueConverters.Add(x => x is DateTime d ? new ObjectValue(d) : null);

                options.Filters.AddFilter("json", JsonFilter.Json);
                options.Filters.AddFilter("jsonparse", JsonParseFilter.JsonParse);
            })
            .AddLiquidFilter<TimeZoneFilter>("local")
            .AddLiquidFilter<SlugifyFilter>("slugify")
            .AddLiquidFilter<LiquidFilter>("liquid")
            .AddLiquidFilter<ContentUrlFilter>("href")
            .AddLiquidFilter<AbsoluteUrlFilter>("absolute_url")
            .AddLiquidFilter<NewShapeFilter>("shape_new")
            .AddLiquidFilter<ShapeRenderFilter>("shape_render")
            .AddLiquidFilter<ShapeStringifyFilter>("shape_stringify");
        }
    }

    [RequireFeatures("OrchardCore.Contents")]
    public class LiquidPartStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Liquid Part
            services.AddScoped<IShapeTableProvider, LiquidShapes>();
            services.AddContentPart<LiquidPart>()
                .UseDisplayDriver<LiquidPartDisplayDriver>()
                .AddHandler<LiquidPartHandler>();

            services.AddTransient<IDataMigration, Migrations>();
            services.AddScoped<IContentPartIndexHandler, LiquidPartIndexHandler>();
        }
    }

    [RequireFeatures("OrchardCore.Shortcodes")]
    public class ShortcodesStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddLiquidFilter<ShortcodeFilter>("shortcode");
        }
    }
}
