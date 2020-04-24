using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Html.Models;
using OrchardCore.Html.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.Html.Handlers
{
    public class HtmlBodyPartHandler : ContentPartHandler<HtmlBodyPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private IHtmlContent _bodyAspect;
        private int _contentItemId;

        public HtmlBodyPartHandler(ILiquidTemplateManager liquidTemplateManager, HtmlEncoder htmlEncoder)
        {
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

                _bodyAspect = bodyAspect.Body;
                _contentItemId = part.ContentItem.Id;

                try
                {
                    var model = new HtmlBodyPartViewModel()
                    {
                        Html = part.Html,
                        HtmlBodyPart = part,
                        ContentItem = part.ContentItem
                    };

                    var result = await _liquidTemplateManager.RenderAsync(part.Html, _htmlEncoder, model,
                        scope => scope.SetValue("ContentItem", model.ContentItem));

                    bodyAspect.Body = _bodyAspect = new HtmlString(result);
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }
    }
}
