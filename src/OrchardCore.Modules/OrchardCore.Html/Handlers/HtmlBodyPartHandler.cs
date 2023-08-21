using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Html.Models;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.Liquid;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Html.Handlers
{
    public class HtmlBodyPartHandler : ContentPartHandler<HtmlBodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShortcodeService _shortcodeService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;

        public HtmlBodyPartHandler(IContentDefinitionManager contentDefinitionManager,
            IShortcodeService shortcodeService,
            ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _shortcodeService = shortcodeService;
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, HtmlBodyPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
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
                            ContentItem = part.ContentItem,
                        };

                        html = await _liquidTemplateManager.RenderStringAsync(html, _htmlEncoder, model,
                            new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });
                    }

                    html = await _shortcodeService.ProcessAsync(html,
                        new Context
                        {
                            ["ContentItem"] = part.ContentItem,
                            ["TypePartDefinition"] = contentTypePartDefinition
                        });

                    bodyAspect.Body = new HtmlString(html);
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }
    }
}
