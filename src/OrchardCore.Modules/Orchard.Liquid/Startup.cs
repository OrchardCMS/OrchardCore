using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migration;
using Orchard.Indexing;
using Orchard.Liquid.Drivers;
using Orchard.Liquid.Filters;
using Orchard.Liquid.Handlers;
using Orchard.Liquid.Indexing;
using Orchard.Liquid.Model;

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
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Liquid Part
            services.AddScoped<IContentPartDisplayDriver, LiquidPartDisplay>();
            services.AddSingleton<ContentPart, LiquidPart>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartIndexHandler, LiquidPartIndexHandler>();
            services.AddScoped<IContentPartHandler, LiquidPartHandler>();

            services.AddScoped<ITemplateContextHandler, MetadataFilters>();
            services.AddScoped<ITemplateContextHandler, UrlContentFilter>();
        }
    }
}
