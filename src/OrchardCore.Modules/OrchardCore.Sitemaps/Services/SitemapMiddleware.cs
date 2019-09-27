using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SitemapEntries _sitemapEntries;
        private readonly ILogger _logger;

        public SitemapMiddleware(
            RequestDelegate next,
            SitemapEntries sitemapEntries,
            ILogger<SitemapMiddleware> logger
            )

        {
            _next = next;
            _logger = logger;
            _sitemapEntries = sitemapEntries;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // TODO remove / maybe. Just for debugging.
            if (!context.Request.Path.Value.EndsWith(".xml"))
            {
                _logger.LogDebug("Request path does not match .xml");
                await _next(context);
                return;
            }
            var endpoint = context.GetEndpoint();
            var meta = endpoint?.Metadata;
            var routes = new RouteValueDictionary()
                {
                    {"Area", "OrchardCore.Contents" },
                    {"Controller", "Item" },
                    {"Action", "Display" },
                    { "ContentItemId", "44f7rpwszm4sj33axcaevw4nws" }
                };
            var _linkGenerator = context.RequestServices.GetRequiredService<LinkGenerator>();
            var link = _linkGenerator.GetPathByAction(routes["Action"].ToString(), routes["Controller"].ToString(), routes);
            link = _linkGenerator.GetPathByName(context, "Display", routes);
            link = _linkGenerator.GetPathByAction(context, routes["Action"].ToString(), routes["Controller"].ToString(), routes);
            link = _linkGenerator.GetPathByRouteValues(context, "any", routes);
            if (endpoint != null && meta != null && routes != null && link != null)
            {

            }

            if (_sitemapEntries.TryGetSitemapNodeId(context.Request.Path.ToString().TrimEnd('/'), out var sitemapNodeId))
            {
                var sitemapSetService = context.RequestServices.GetRequiredService<ISitemapSetService>();
                var sitemapBuilder = context.RequestServices.GetRequiredService<ISitemapBuilder>();
                var sitemapNode = await sitemapSetService.GetSitemapNodeByIdAsync(sitemapNodeId);
                //var act = context.RequestServices.GetRequiredService<IActionContextAccessor>();
                //var t = context.RequestServices.GetRequiredService<IUrlHelperFactory>();
                //var urlHelper = t.GetUrlHelper(act.ActionContext);


                //_linkGenerator.
                //var factory = context.RequestServices.GetRequiredService<IUrlHelperFactory>();

                //var actionContext = new ActionContext(context, context.GetRouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
                //var urlHelper = factory.GetUrlHelper(actionContext);
                //var urllink = urlHelper.Action(routes["Action"].ToString(), routes);
                //if (urllink != null)
                //{

                //}

                var request = context.Request;
                var scheme = request.Scheme;
                var host = request.Host.ToUriComponent();
                var prefixUrl = $"{scheme}://{host}{request.PathBase}";

                var builderContext = new SitemapBuilderContext()
                {
                    PrefixUrl = prefixUrl,
                    Builder = sitemapBuilder,
                    //Url = urlHelper
                };

                var document = await sitemapBuilder.BuildAsync(sitemapNode, builderContext);

                document.Declaration = new XDeclaration("1.0", "utf-8", null);
                StringWriter writer = new Utf8StringWriter();
                document.Save(writer, SaveOptions.None);
                //TODO check size for > 10MB and log or move these type of checks into a ValidateAsync as part of google ping
                //return Content(writer.ToString(), "application/xml", Encoding.UTF8);

                context.Response.ContentType = "text/xml";
                await context.Response.WriteAsync(writer.ToString());
                context.Response.StatusCode = 200;
                return;
            }

            await _next(context);
        }


        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}
