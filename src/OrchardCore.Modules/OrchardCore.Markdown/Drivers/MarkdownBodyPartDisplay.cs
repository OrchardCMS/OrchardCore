using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Drivers
{
    public class MarkdownBodyPartDisplay : ContentPartDisplayDriver<MarkdownBodyPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IStringLocalizer<MarkdownBodyPartDisplay> S;

        public MarkdownBodyPartDisplay(ILiquidTemplateManager liquidTemplateManager, IStringLocalizer<MarkdownBodyPartDisplay> localizer, HtmlEncoder htmlEncoder)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
            S = localizer;
        }

        public override IDisplayResult Display(MarkdownBodyPart MarkdownBodyPart, BuildPartDisplayContext context)
        {
            return Initialize<MarkdownBodyPartViewModel>("MarkdownBodyPart", m => BuildViewModel(m, MarkdownBodyPart))
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

        private async ValueTask BuildViewModel(MarkdownBodyPartViewModel model, MarkdownBodyPart MarkdownBodyPart)
        {
            model.Markdown = MarkdownBodyPart.Markdown;
            model.MarkdownBodyPart = MarkdownBodyPart;
            model.ContentItem = MarkdownBodyPart.ContentItem;

            model.Markdown = await _liquidTemplateManager.RenderAsync(MarkdownBodyPart.Markdown, _htmlEncoder, model,
                scope => scope.SetValue("ContentItem", model.ContentItem));

            model.Html = Markdig.Markdown.ToHtml(model.Markdown ?? "");
        }
    }
}
