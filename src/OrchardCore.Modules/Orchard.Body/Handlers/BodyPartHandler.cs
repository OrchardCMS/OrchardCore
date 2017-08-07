using System.Linq;
using Microsoft.AspNetCore.Html;
using Orchard.Body.Model;
using Orchard.Body.Settings;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Models;

namespace Orchard.Body.Handlers
{
    public class BodyPartHandler : ContentPartHandler<BodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public BodyPartHandler(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override void GetContentItemAspect(ContentItemAspectContext context, BodyPart part)
        {
            context.For<BodyAspect>(bodyAspect =>
            {

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(BodyPart));
                var settings = contentTypePartDefinition.GetSettings<BodyPartSettings>();

                var body = part.Body;

                bodyAspect.Body = new HtmlString(body);
            });
        }
    }
}