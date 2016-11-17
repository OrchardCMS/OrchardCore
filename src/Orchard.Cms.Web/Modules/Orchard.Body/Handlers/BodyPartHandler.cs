using Microsoft.AspNetCore.Html;
using Orchard.Body.Model;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Models;

namespace Orchard.Body.Handlers
{
    public class BodyPartHandler : ContentPartHandler<BodyPart>
    {
        public override void GetContentItemAspect(ContentItemAspectContext context, BodyPart part)
        {
            context.For<BodyAspect>(bodyAspect => bodyAspect.Body = new HtmlString(part.Body));
        }
    }
}