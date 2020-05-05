using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Infrastructure.SafeCodeFilters;
using OrchardCore.Infrastructure.Script;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Drivers
{
    public class MarkdownBodyPartDisplay : ContentPartDisplayDriver<MarkdownBodyPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IHtmlScriptSanitizer _htmlScriptSanitizer;
        private readonly ISafeCodeFilterManager _safeCodeFilterManager;
        private readonly IStringLocalizer S;

        public MarkdownBodyPartDisplay(ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder,
            IHtmlScriptSanitizer htmlScriptSanitizer,
            ISafeCodeFilterManager safeCodeFilterManager,
            IStringLocalizer<MarkdownBodyPartDisplay> localizer)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
            _htmlScriptSanitizer = htmlScriptSanitizer;
            _safeCodeFilterManager = safeCodeFilterManager;
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

            if (settings.AllowCustomScripts)
            {
                model.Markdown = await _liquidTemplateManager.RenderAsync(markdownBodyPart.Markdown, _htmlEncoder, model,
                    scope => scope.SetValue("ContentItem", model.ContentItem));
            }

            model.Markdown = await _safeCodeFilterManager.ProcessAsync(model.Markdown ?? "");
            model.Html = Markdig.Markdown.ToHtml(model.Markdown ?? "");

            if (!settings.AllowCustomScripts)
            {
                model.Html = _htmlScriptSanitizer.Sanitize(model.Html ?? "");
            }
        }
    }
}
