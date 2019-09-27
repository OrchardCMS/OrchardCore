using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly SitemapEntries _sitemapEntries;
        private readonly ISitemapBuilder _sitemapBuilder;
        private readonly ISitemapSetService _sitemapSetService;
        private readonly ILogger _logger;
        public SitemapsController(
            ILogger<SitemapsController> logger,
            SitemapEntries sitemapEntries,
            ISitemapBuilder sitemapBuilder,
            ISitemapSetService sitemapSetService
            )
        {
            _logger = logger;
            _sitemapEntries = sitemapEntries;
            _sitemapBuilder = sitemapBuilder;
            _sitemapSetService = sitemapSetService;
        }

        public async Task<IActionResult> Index()
        {
            var sitemapPath = HttpContext.GetRouteValue(SitemapRouteConstraint.RouteKey)?.ToString();
            _logger.LogDebug("Sitemap path {SitemapPath}", sitemapPath);

            if (_sitemapEntries.TryGetSitemapNodeId(sitemapPath, out var sitemapNodeId))
            {
                var sitemapNode = await _sitemapSetService.GetSitemapNodeByIdAsync(sitemapNodeId);

                var context = new SitemapBuilderContext()
                {
                    UrlHelper = Url,
                    Builder = _sitemapBuilder
                };

                var document = await _sitemapBuilder.BuildAsync(sitemapNode, context);

                document.Declaration = new XDeclaration("1.0", "utf-8", null);
                StringWriter writer = new Utf8StringWriter();
                document.Save(writer, SaveOptions.None);
                //TODO check size for > 10MB and log or move these type of checks into a ValidateAsync as part of google ping
                return Content(writer.ToString(), "application/xml", Encoding.UTF8);
            };

            return NotFound();
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}

