using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Html.Model;
using OrchardCore.Html.Settings;

namespace OrchardCore.Html.Handlers
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
            return context.ForAsync<BodyAspect>(bodyAspect =>
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(HtmlBodyPart));
                var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartSettings>();
                var body = part.Html;

                bodyAspect.Body = new HtmlString(body);
                return Task.CompletedTask;
            });
        }
    }
}
