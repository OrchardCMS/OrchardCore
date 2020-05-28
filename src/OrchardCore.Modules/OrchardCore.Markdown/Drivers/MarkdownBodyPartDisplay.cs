using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ShortCodes.Services;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Drivers
{
    public class MarkdownBodyPartDisplay : ContentPartDisplayDriver<MarkdownBodyPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IShortCodeService _shortCodeService;
        private readonly IMarkdownService _markdownService;
        private readonly IStringLocalizer S;

        public MarkdownBodyPartDisplay(ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder,
            IHtmlSanitizerService htmlSanitizerService,
            IShortCodeService shortCodeService,
            IMarkdownService markdownService,
            IStringLocalizer<MarkdownBodyPartDisplay> localizer)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
            _htmlSanitizerService = htmlSanitizerService;
            _shortCodeService = shortCodeService;
            _markdownService = markdownService;
            S = localizer;
        }

        public override IDisplayResult Display(MarkdownBodyPart markdownBodyPart, BuildPartDisplayContext context)
        {
            return Initialize<MarkdownBodyPartViewModel>(GetDisplayShapeType(context), m => BuildViewModel(m, markdownBodyPart, context.TypePartDefinition.GetSettings<MarkdownBodyPartSettings>()))
                .Location("Detail", "Content:10")
                .Location("Summary", "Content:10");
        }

        public override IDisplayResult Edit(MarkdownBodyPart markdownBodyPart, BuildPartEditorContext context)
        {
            return Initialize<MarkdownBodyPartViewModel>(GetEditorShapeType(context), model =>
            {
                model.Markdown = markdownBodyPart.Markdown;
                model.ContentItem = markdownBodyPart.ContentItem;
                model.MarkdownBodyPart = markdownBodyPart;
                model.TypePartDefinition = context.TypePartDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MarkdownBodyPart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new MarkdownBodyPartViewModel();

            if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Markdown))
            {
                if (!string.IsNullOrEmpty(viewModel.Markdown) && !_liquidTemplateManager.Validate(viewModel.Markdown, out var errors))
                {
                    var partName = context.TypePartDefinition.DisplayName();
                    context.Updater.ModelState.AddModelError(nameof(model.Markdown), S["{0} doesn't contain a valid Liquid expression. Details: {1}", partName, string.Join(" ", errors)]);
                }
                else
                {
                    model.Markdown = viewModel.Markdown;
                }
            }

            return Edit(model, context);
        }

        private async ValueTask BuildViewModel(MarkdownBodyPartViewModel model, MarkdownBodyPart markdownBodyPart, MarkdownBodyPartSettings settings)
        {
            model.Markdown = markdownBodyPart.Markdown;
            model.MarkdownBodyPart = markdownBodyPart;
            model.ContentItem = markdownBodyPart.ContentItem;

            // The default Markdown option is to entity escape html
            // so filters must be run after the markdown has been processed.
            model.Html = _markdownService.ToHtml(model.Markdown ?? "");

            // The liquid rendering is for backwards compatability and can be removed in a future version.
            if (!settings.SanitizeHtml)
            {
                model.Html = await _liquidTemplateManager.RenderAsync(model.Html, _htmlEncoder, model,
                    scope => scope.SetValue("ContentItem", model.ContentItem));
            }

            model.Html = await _shortCodeService.ProcessAsync(model.Html ?? "");

            if (settings.SanitizeHtml)
            {
                model.Html = _htmlSanitizerService.Sanitize(model.Html ?? "");
            }
        }
    }
}
