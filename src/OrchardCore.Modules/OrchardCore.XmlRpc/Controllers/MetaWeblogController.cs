using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.XmlRpc.Controllers
{
    [Feature("OrchardCore.RemotePublishing")]
    public class MetaWeblogController : Controller
    {
        private const string ManifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";

        private readonly IEnumerable<IXmlRpcHandler> _xmlRpcHandlers;
        private readonly ILogger _logger;

        public MetaWeblogController(
            IEnumerable<IXmlRpcHandler> xmlRpcHandlers,
            ILogger<MetaWeblogController> logger)
        {
            _xmlRpcHandlers = xmlRpcHandlers;
            _logger = logger;
        }

        [ResponseCache(Duration = 0, NoStore = true)]
        public ActionResult Manifest()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Manifest requested");
            }

            var options = new XElement(
                XName.Get("options", ManifestUri),
                new XElement(XName.Get("supportsAutoUpdate", ManifestUri), "Yes"),
                new XElement(XName.Get("clientType", ManifestUri), "MetaWeblog"),
                new XElement(XName.Get("supportsKeywords", ManifestUri), "No"),
                new XElement(XName.Get("supportsCategories", ManifestUri), "No"),
                new XElement(XName.Get("supportsFileUpload", ManifestUri), "No"),
                new XElement(XName.Get("supportsCustomDate", ManifestUri), "No"));

            foreach (var handler in _xmlRpcHandlers)
            {
                handler.SetCapabilities(options);
            }

            var doc = new XDocument(
                        new XElement(
                            XName.Get("manifest", ManifestUri),
                            options));

            return Content(doc.ToString(), "text/xml");
        }
    }
}
