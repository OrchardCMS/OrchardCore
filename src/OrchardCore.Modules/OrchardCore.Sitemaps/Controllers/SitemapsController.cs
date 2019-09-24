using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Routing;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Controllers
{
    public class SitemapsController : Controller
    {
        private readonly ISitemapRoute _sitemapRoute;
        private readonly ISitemapBuilder _sitemapBuilder;
        private readonly ISitemapSetService _sitemapSetService;
        public SitemapsController(
            ILogger<SitemapsController> logger,
            ISitemapRoute sitemapRoute,
            ISitemapBuilder sitemapBuilder,
            ISitemapSetService sitemapSetService
            )
        {
            Logger = logger;
            _sitemapRoute = sitemapRoute;
            _sitemapBuilder = sitemapBuilder;
            _sitemapSetService = sitemapSetService;
        }

        public ILogger Logger { get; }

        public async Task<IActionResult> Index()
        {
            var sitemapPath = HttpContext.GetRouteValue(SitemapRouteConstraint.RouteKey)?.ToString();
            Logger.LogDebug($"Sitemap path {sitemapPath}");

            var sitemapNodeId = await _sitemapRoute.GetSitemapNodeByPathAsync(sitemapPath);
            var sitemapNode = await _sitemapSetService.GetSitemapNodeByIdAsync(sitemapNodeId);
            //this controllers UrlHelper does not contain the AutoRoute router (we're in a different ActionContext?)
            //so construct a urlhelper with good RouteData. Suspect may change again with EndPoint routing anyway
            var actionContext = new ActionContext(HttpContext, HttpContext.GetRouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
            var url = new UrlHelper(actionContext);
            var context = new SitemapBuilderContext()
            {
                Url = url,
                Builder = _sitemapBuilder
            };
            var document = await _sitemapBuilder.BuildAsync(sitemapNode, context);

            document.Declaration = new XDeclaration("1.0", "utf-8", null);
            StringWriter writer = new Utf8StringWriter();
            document.Save(writer, SaveOptions.None);
            //TODO check size for > 10MB and log or move these type of checks into a ValidateAsync as part of google ping
            return Content(writer.ToString(), "application/xml", Encoding.UTF8);
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}

