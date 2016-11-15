using System;
using Microsoft.AspNetCore.Html;
using Orchard.Body.Model;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Models;

namespace Orchard.Body.Handlers
{
    public class BodyPartHandler : ContentPartHandler<BodyPart>
    {
        public override void GetContentAspect(ContentItemAspectContext context, BodyPart part)
        {
            var bodyAspect = context.Aspect as IBodyAspect;

            if (bodyAspect != null)
            {
                bodyAspect.Body = new HtmlString(part.Body);
            }
        }
    }
}