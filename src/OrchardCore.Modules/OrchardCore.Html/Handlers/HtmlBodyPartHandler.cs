using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
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

        private HtmlString _bodyAspect;

        public HtmlBodyPartHandler(ILiquidTemplateManager liquidTemplateManager)
        {
            _liquidTemplateManager = liquidTemplateManager;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, HtmlBodyPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
                if (_bodyAspect != null)
                {
                    bodyAspect.Body = _bodyAspect;
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

                    var templateContext = new TemplateContext();
                    templateContext.SetValue("ContentItem", part.ContentItem);
                    templateContext.MemberAccessStrategy.Register<HtmlBodyPartViewModel>();
                    templateContext.SetValue("Model", model);

                    var result = await _liquidTemplateManager.RenderAsync(part.Html, HtmlEncoder.Default, templateContext);
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
