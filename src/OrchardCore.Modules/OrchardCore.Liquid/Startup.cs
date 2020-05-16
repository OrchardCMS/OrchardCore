using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Views;
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
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<ContentItem>();
            TemplateContext.GlobalMemberAccessStrategy.Register<ContentElement>();
            TemplateContext.GlobalMemberAccessStrategy.Register<ShapeViewModel<ContentItem>>();
            TemplateContext.GlobalMemberAccessStrategy.Register<ContentTypePartDefinition>();
            TemplateContext.GlobalMemberAccessStrategy.Register<ContentPartFieldDefinition>();
            TemplateContext.GlobalMemberAccessStrategy.Register<ContentFieldDefinition>();
            TemplateContext.GlobalMemberAccessStrategy.Register<ContentPartDefinition>();

            // When accessing a property of a JObject instance
            TemplateContext.GlobalMemberAccessStrategy.Register<JObject, object>((obj, name) => obj[name]);

            // Prevent JTokens from being converted to an ArrayValue as they implement IEnumerable
            FluidValue.SetTypeMapping<JObject>(o => new ObjectValue(o));
            FluidValue.SetTypeMapping<JValue>(o => FluidValue.Create(((JValue)o).Value));
            FluidValue.SetTypeMapping<System.DateTime>(o => new ObjectValue(o));
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISlugService, SlugService>();
            services.AddScoped<ILiquidTemplateManager, LiquidTemplateManager>();

            services.AddLiquidFilter<TimeZoneFilter>("local");
            services.AddLiquidFilter<SlugifyFilter>("slugify");
            services.AddLiquidFilter<ContainerFilter>("container");
            services.AddLiquidFilter<DisplayTextFilter>("display_text");
            services.AddLiquidFilter<DisplayUrlFilter>("display_url");
            services.AddLiquidFilter<ContentUrlFilter>("href");
            services.AddLiquidFilter<AbsoluteUrlFilter>("absolute_url");
            services.AddLiquidFilter<LiquidFilter>("liquid");
            services.AddLiquidFilter<JsonFilter>("json");
            services.AddLiquidFilter<JsonParseFilter>("jsonparse");
            services.AddLiquidFilter<ShortCodeFilter>("shortcode");
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
                .UseDisplayDriver<LiquidPartDisplay>()
                .AddHandler<LiquidPartHandler>();

            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartIndexHandler, LiquidPartIndexHandler>();
        }
    }
}
