using System.Collections.Generic;
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
        private readonly IDictionary<string, HtmlString> _bodiesAspectDictionary = new Dictionary<string, HtmlString>();

        public HtmlBodyPartHandler(ILiquidTemplateManager liquidTemplateManager, HtmlEncoder htmlEncoder)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, HtmlBodyPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
                var contentItemVersionId = part.ContentItem.ContentItemVersionId;
                if (_bodiesAspectDictionary.ContainsKey(contentItemVersionId))
                {
                    bodyAspect.Body = _bodiesAspectDictionary[contentItemVersionId];

                    return;
                }

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

                    _bodiesAspectDictionary.Add(contentItemVersionId, new HtmlString(result));
                    bodyAspect.Body = _bodiesAspectDictionary[contentItemVersionId];
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }
    }
}
