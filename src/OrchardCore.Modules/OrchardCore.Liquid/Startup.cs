using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.Indexing;
using OrchardCore.Liquid.Drivers;
using OrchardCore.Liquid.Filters;
using OrchardCore.Liquid.Handlers;
using OrchardCore.Liquid.Indexing;
using OrchardCore.Liquid.Models;
using OrchardCore.Liquid.Services;
using OrchardCore.Liquid.ViewModels;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Liquid;

public sealed class Startup : StartupBase
{
    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.UseMiddleware<ScriptsMiddleware>();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ILiquidTemplateManager, LiquidTemplateManager>();

        services.Configure<TemplateOptions>(options =>
        {
            options.Filters.AddFilter("t", LiquidViewFilters.Localize);
            options.Filters.AddFilter("html_class", LiquidViewFilters.HtmlClass);
            options.Filters.AddFilter("shape_properties", LiquidViewFilters.ShapeProperties);

            options.MemberAccessStrategy.Register<LiquidPartViewModel>();

            // Used to provide a factory to return a value based on a property name that is unknown at registration time.
            options.MemberAccessStrategy.Register<LiquidPropertyAccessor, FluidValue>((obj, name) => obj.GetValueAsync(name));

            // When a property of a 'JsonObject' value is accessed, try to look into its properties.
            options.MemberAccessStrategy.Register<JsonObject, object>((source, name) => source[name]);

            // When a property of a 'JsonDynamicObject' value is accessed, try to look into its properties.
            options.MemberAccessStrategy.Register<JsonDynamicObject, object>((json, name) => json[name]);

            // Convert JToken to FluidValue
            options.ValueConverters.Add(x =>
            {
                return x switch
                {
                    JsonObject o => new ObjectValue(o),
                    JsonDynamicObject o => new ObjectValue((JsonObject)o),
                    JsonValue o => o.GetObjectValue(),
                    JsonDynamicValue o => ((JsonValue)(o.Node)).GetObjectValue(),
                    DateTime d => new ObjectValue(d),
                    _ => null

                };
            });

            options.Filters.AddFilter("json", JsonFilter.Json);
            options.Filters.AddFilter("jsonparse", JsonParseFilter.JsonParse);
        })
        .AddLiquidFilter<LocalTimeZoneFilter>("local")
        .AddLiquidFilter<UtcTimeZoneFilter>("utc")
        .AddLiquidFilter<SlugifyFilter>("slugify")
        .AddLiquidFilter<LiquidFilter>("liquid")
        .AddLiquidFilter<ContentUrlFilter>("href")
        .AddLiquidFilter<AbsoluteUrlFilter>("absolute_url")
        .AddLiquidFilter<NewShapeFilter>("shape_new")
        .AddLiquidFilter<ShapeRenderFilter>("shape_render")
        .AddLiquidFilter<ShapeStringifyFilter>("shape_stringify");

        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
    }
}

[RequireFeatures("OrchardCore.Contents")]
public sealed class LiquidPartStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Liquid Part
        services.AddShapeTableProvider<LiquidShapes>();
        services.AddContentPart<LiquidPart>()
            .UseDisplayDriver<LiquidPartDisplayDriver>()
            .AddHandler<LiquidPartHandler>();

        services.AddDataMigration<Migrations>();
        services.AddScoped<IContentPartIndexHandler, LiquidPartIndexHandler>();
    }
}

[RequireFeatures("OrchardCore.Shortcodes")]
public sealed class ShortcodesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddLiquidFilter<ShortcodeFilter>("shortcode");
    }
}
