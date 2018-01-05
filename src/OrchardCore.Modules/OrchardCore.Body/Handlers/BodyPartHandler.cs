using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.Body.Model;
using OrchardCore.Body.Settings;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;

namespace OrchardCore.Body.Handlers
{
    public class BodyPartHandler : ContentPartHandler<BodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public BodyPartHandler(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, BodyPart part)
        {
            context.For<BodyAspect>(bodyAspect =>
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(BodyPart));
                var settings = contentTypePartDefinition.GetSettings<BodyPartSettings>();
                var body = part.Body;

                bodyAspect.Body = new HtmlString(body);
            });

            return Task.CompletedTask;
        }
    }
}