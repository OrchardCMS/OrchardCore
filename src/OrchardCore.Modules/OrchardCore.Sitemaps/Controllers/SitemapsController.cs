using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Controllers
{
    public class SitemapsController : Controller
    {
        private readonly ISitemapManager _sitemapManager;
        private readonly ISitemapSetService _sitemapSetService;

        private readonly ISitemapBuilder _sitemapBuilder;

        public SitemapsController(
            ILogger<SitemapsController> logger,
            ISitemapManager sitemapManager,
            ISitemapSetService sitemapSetService,
            ISitemapBuilder sitemapBuilder
            )
        {
            Logger = logger;
            _sitemapManager = sitemapManager;
            _sitemapSetService = sitemapSetService;
            _sitemapBuilder = sitemapBuilder;
        }

        public ILogger Logger { get; set; }

        public async Task<IActionResult> Index()
        {
            var sitemapPath = HttpContext.GetRouteValue(SitemapRouteConstraint.RouteKey)?.ToString();
            Logger.LogDebug($"Sitemap path {sitemapPath}");

            var sitemapNode = await _sitemapSetService.GetSitemapNodeByPathAsync(sitemapPath);

            var context = new SitemapBuilderContext();
            //so what will this build on. propably a context
            var document = await _sitemapBuilder.BuildAsync(sitemapNode, context);

            return Content(document.ToString(), "text/xml");
            //var sitemap = await _sitemapManager.BuildSitemap(number);

            //var ns = new XmlSerializerNamespaces();
            //ns.Add("", "http://www.sitemaps.org/schemas/sitemap/0.9");
            //var ser = new XmlSerializer(sitemap.GetType());
            //using (var outStream = new Utf8StringWriter()) {
            //    ser.Serialize(outStream, sitemap, ns);

            //    return Content(outStream.ToString(), "application/xml");
            //}
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

    }
}

