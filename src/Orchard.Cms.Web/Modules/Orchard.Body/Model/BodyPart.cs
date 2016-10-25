using Microsoft.AspNetCore.Html;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Models;

namespace Orchard.Body.Model
{
    public class BodyPart : ContentPart, IBody
    {
        public string Body { get; set; }

        public IHtmlContent BodyHtml { get { return new HtmlContentBuilder().SetHtmlContent(Body); } }
    }
}
