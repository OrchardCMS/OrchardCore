using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Controllers
{
    [RequireFeatures("OrchardCore.RemotePublishing")]
    public class RemotePublishingController : Controller
    {
        private readonly IContentManager _contentManager;

        public RemotePublishingController(
            IContentManager contentManager,
            ILogger<RemotePublishingController> logger)
        {
            _contentManager = contentManager;
            Logger = logger;
        }

        ILogger Logger { get; set; }

        public async Task<IActionResult> Rsd(string contentItemId)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("RSD requested");
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
                new XElement(XName.Get("engineLink", manifestUri), "https://orchardproject.net"),
                new XElement(XName.Get("homePageLink", manifestUri), "https://orchardproject.net"),
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