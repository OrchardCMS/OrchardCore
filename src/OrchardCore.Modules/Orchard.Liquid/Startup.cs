using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migration;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Indexing;
using Orchard.Liquid.Drivers;
using Orchard.Liquid.Filters;
using Orchard.Liquid.Handlers;
using Orchard.Liquid.Indexing;
using Orchard.Liquid.Model;
using Orchard.Liquid.Services;

namespace Orchard.Liquid
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<ContentItem>();

            // When accessing a property of a JObject instance
            TemplateContext.GlobalMemberAccessStrategy.Register<JObject>((obj, name) => obj[name]);

            // Prevent JTokens from being converted to an ArrayValue as they implement IEnumerable
            FluidValue.TypeMappings.Add(typeof(JObject), o => new ObjectValue(o));
            FluidValue.TypeMappings.Add(typeof(JValue), o => FluidValue.Create(((JValue)o).Value));
            FluidValue.TypeMappings.Add(typeof(System.DateTime), o => new ObjectValue(o));
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Liquid Part
            services.AddScoped<IContentPartDisplayDriver, LiquidPartDisplay>();
            services.AddScoped<IShapeTableProvider, LiquidShapes>();
            services.AddSingleton<ContentPart, LiquidPart>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartIndexHandler, LiquidPartIndexHandler>();
            services.AddScoped<IContentPartHandler, LiquidPartHandler>();

            services.AddScoped<ISlugService, SlugService>();

            services.AddScoped<ILiquidTemplateManager, LiquidTemplateManager>();

            services.AddLiquidFilter<TimeZoneFilter>("local");
            services.AddLiquidFilter<SlugifyFilter>("slugify");
            services.AddLiquidFilter<ContainerFilter>("container");
            services.AddLiquidFilter<DisplayTextFilter>("display_text");
            services.AddLiquidFilter<DisplayUrlFilter>("display_url");
            services.AddLiquidFilter<ContentUrlFilter>("href");
        }
    }
}
