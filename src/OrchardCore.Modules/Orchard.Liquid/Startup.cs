using System.Linq;
using System.Net;
using System.Security.Principal;
using DotLiquid;
using DotLiquid.NamingConventions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migration;
using Orchard.Indexing;
using Orchard.Liquid.Drivers;
using Orchard.Liquid.Drops;
using Orchard.Liquid.Filters;
using Orchard.Liquid.Handlers;
using Orchard.Liquid.Indexing;
using Orchard.Liquid.Model;

namespace Orchard.Liquid
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddLiquid();

            // Liquid Part
            services.AddScoped<IContentPartDisplayDriver, LiquidPartDisplay>();
            services.AddSingleton<ContentPart, LiquidPart>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartIndexHandler, LiquidPartIndexHandler>();
            services.AddScoped<IContentPartHandler, LiquidPartHandler>();

            Template.RegisterSafeType(typeof(IContent), new string[] { nameof(IContent.ContentItem) });
            Template.RegisterSafeType(typeof(ContentItem), o => new ContentItemDrop((ContentItem)o));

            Template.RegisterSafeType(typeof(JToken), o => new JTokenDrop((JToken)o));
            Template.RegisterSafeType(typeof(JObject), o => new JTokenDrop((JToken)o));
            Template.RegisterSafeType(typeof(JArray), o => new JArrayDrop((JArray)o));

            Template.RegisterSafeType(typeof(ContentElement), new string[] {
                nameof(ContentElement.Content),
                nameof(ContentElement.ContentItem)
            });
            Template.RegisterSafeType(typeof(DefaultHttpContext), new string[] {
                nameof(HttpContext.User),
                nameof(HttpContext.Request)
            });
            Template.RegisterSafeType(typeof(DefaultHttpRequest), new string[] {
                nameof(HttpRequest.Path),
                nameof(HttpRequest.PathBase),
                nameof(HttpRequest.Host),
                nameof(HttpRequest.IsHttps),
                nameof(HttpRequest.Method),
                nameof(HttpRequest.Protocol),
                nameof(HttpRequest.Query),
                nameof(HttpRequest.QueryString),
                nameof(HttpRequest.Form),
                nameof(HttpRequest.Cookies),
                nameof(HttpRequest.Headers)
            });

            Template.RegisterSafeType(typeof(HostString), p => p.ToString());
            Template.RegisterSafeType(typeof(PathString), p => p.ToString());
            Template.RegisterSafeType(typeof(QueryString), p => p.ToString());
            Template.RegisterSafeType(typeof(IQueryCollection), p => ((IQueryCollection)p).ToDictionary(x => x.Key, y => y.Value));
            Template.RegisterSafeType(typeof(IFormCollection), p => ((IFormCollection)p).ToDictionary(x => x.Key, y => y.Value));
            Template.RegisterSafeType(typeof(IRequestCookieCollection), p => ((IRequestCookieCollection)p).ToDictionary(x => x.Key, y => y.Value));
            Template.RegisterSafeType(typeof(IHeaderDictionary), p => ((IHeaderDictionary)p).ToDictionary(x => x.Key, y => y.Value));

            Template.RegisterSafeType(typeof(IPrincipal), new string[] { nameof(IPrincipal.Identity) });
            Template.RegisterSafeType(typeof(IIdentity), new string[] { nameof(IIdentity.Name) });

            DotLiquid.Liquid.UseRubyDateFormat = false;

            Template.NamingConvention = new CSharpNamingConvention();
            Template.RegisterFilter(typeof(MetadataFilters));
            
            // Html encoding support
            Template.RegisterValueTypeTransformer(typeof(string), m => WebUtility.HtmlEncode((string)m));
            Template.RegisterValueTypeTransformer(typeof(IHtmlContent), m => m.ToString());
        }
    }
}
