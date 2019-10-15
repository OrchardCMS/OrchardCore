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
        private readonly ISitemapManager _sitemapManager;
        private readonly ILogger _logger;
        public SitemapController(
            ILogger<SitemapController> logger,
            SitemapEntries sitemapEntries,
            ISitemapManager sitemapManager
            )
        {
            _logger = logger;
            _sitemapEntries = sitemapEntries;
            _sitemapManager = sitemapManager;
        }

        public async Task<IActionResult> Index()
        {
            var sitemapPath = HttpContext.GetRouteValue(SitemapsTransformer.RouteKey)?.ToString();
            _logger.LogDebug("Sitemap request path {SitemapPath}", sitemapPath);

            if (_sitemapEntries.TryGetSitemapNodeId(sitemapPath, out var sitemapId))
            {
                var sitemap = await _sitemapManager.GetSitemapAsync(sitemapId);

                if (sitemap == null)
                {
                    return NotFound();
                }

                var context = new SitemapBuilderContext()
                {
                    HostPrefix = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}",
                    UrlHelper = Url
                };

                var document = await _sitemapManager.BuildSitemapAsync(sitemap, context);

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

