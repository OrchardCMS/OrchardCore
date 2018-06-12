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
    public class HtmlBodyPartHandler : ContentPartHandler<HtmlBodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public HtmlBodyPartHandler(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, HtmlBodyPart part)
        {
            context.For<BodyAspect>(bodyAspect =>
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(HtmlBodyPart));
                var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartSettings>();
                var body = part.Body;

                bodyAspect.Body = new HtmlString(body);
            });

            return Task.CompletedTask;
        }
    }
}