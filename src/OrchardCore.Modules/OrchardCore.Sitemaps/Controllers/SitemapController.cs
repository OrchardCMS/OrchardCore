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
    public class SitemapController : Controller
    {
        private readonly SitemapEntries _sitemapEntries;
        private readonly ISitemapBuilder _sitemapBuilder;
        private readonly ISitemapService _sitemapService;
        private readonly ILogger _logger;
        public SitemapController(
            ILogger<SitemapController> logger,
            SitemapEntries sitemapEntries,
            ISitemapBuilder sitemapBuilder,
            ISitemapService sitemapService
            )
        {
            _logger = logger;
            _sitemapEntries = sitemapEntries;
            _sitemapBuilder = sitemapBuilder;
            _sitemapService = sitemapService;
        }

        public async Task<IActionResult> Index()
        {
            var sitemapPath = HttpContext.GetRouteValue(SitemapRouteConstraint.RouteKey)?.ToString();
            _logger.LogDebug("Sitemap path {SitemapPath}", sitemapPath);

            if (_sitemapEntries.TryGetSitemapNodeId(sitemapPath, out var sitemapNodeId))
            {
                var sitemapDocument = await _sitemapService.LoadSitemapDocumentAsync();
                var sitemapNode = sitemapDocument.GetSitemapNodeById(sitemapNodeId);

                if (sitemapNode == null)
                {
                    return NotFound();
                }

                var context = new SitemapBuilderContext()
                {
                    HostPrefix = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}",
                    UrlHelper = Url,
                    Builder = _sitemapBuilder
                };

                var document = await _sitemapBuilder.BuildAsync(sitemapNode, context);

                document.Declaration = new XDeclaration("1.0", "utf-8", null);
                StringWriter writer = new Utf8StringWriter();
                document.Save(writer, SaveOptions.None);
                //TODO check size for > 50MB and log or move these type of checks into a ValidateAsync as part of google ping
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

