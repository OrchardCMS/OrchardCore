using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Html.Models;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.ShortCodes.Services;
using OrchardCore.Liquid;

namespace OrchardCore.Html.Handlers
{
    public class HtmlBodyPartHandler : ContentPartHandler<HtmlBodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShortCodeService _shortCodeService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private HtmlString _bodyAspect;
        private int _contentItemId;

        public HtmlBodyPartHandler(IContentDefinitionManager contentDefinitionManager,
            IShortCodeService shortCodeService,
            ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _shortCodeService = shortCodeService;
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, HtmlBodyPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
                if (bodyAspect != null && part.ContentItem.Id == _contentItemId)
                {
                    bodyAspect.Body = _bodyAspect;

                    return;
                }

                try
                {
                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                    var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "HtmlBodyPart"));
                    var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartSettings>();

                    var html = part.Html;
                    if (!settings.SanitizeHtml)
                    {
                        var model = new HtmlBodyPartViewModel()
                        {
                            Html = part.Html,
                            HtmlBodyPart = part,
                            ContentItem = part.ContentItem
                        };

                        html = await _liquidTemplateManager.RenderAsync(html, _htmlEncoder, model,
                            scope => scope.SetValue("ContentItem", model.ContentItem));
                    }

                    html = await _shortCodeService.ProcessAsync(html);

                    bodyAspect.Body = _bodyAspect = new HtmlString(html);
                    _contentItemId = part.ContentItem.Id;
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                    _contentItemId = default;
                }
            });
        }
    }
}
