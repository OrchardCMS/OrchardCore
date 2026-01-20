using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Modules;

namespace OrchardCore.Lists.Controllers
{
    [RequireFeatures("OrchardCore.RemotePublishing")]
    public class RemotePublishingController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly ILogger _logger;

        public RemotePublishingController(
            IContentManager contentManager,
            ILogger<RemotePublishingController> logger)
        {
            _contentManager = contentManager;
            _logger = logger;
        }

        public async Task<IActionResult> Rsd(string contentItemId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("RSD requested");
            }

            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            var listPart = contentItem.As<ListPart>();

            if (listPart == null)
            {
                return NotFound();
            }

            const string manifestUri = "http://archipelago.phrasewise.com/rsd";

            var url = Url.Action("Index", "Home", new { area = "OrchardCore.XmlRpc" }, Request.Scheme);

            var options = new XElement(
                XName.Get("service", manifestUri),
                new XElement(XName.Get("engineName", manifestUri), "Orchard CMS"),
                new XElement(XName.Get("engineLink", manifestUri), "https://orchardcore.net"),
                new XElement(XName.Get("homePageLink", manifestUri), "https://orchardcore.net"),
                new XElement(XName.Get("apis", manifestUri),
                    new XElement(XName.Get("api", manifestUri),
                        new XAttribute("name", "MetaWeblog"),
                        new XAttribute("preferred", true),
                        new XAttribute("apiLink", url),
                        new XAttribute("blogID", contentItem.ContentItemId))));

            var doc = new XDocument(new XElement(
                                        XName.Get("rsd", manifestUri),
                                        new XAttribute("version", "1.0"),
                                        options));

            return Content(doc.ToString(), "text/xml");
        }
    }
}
