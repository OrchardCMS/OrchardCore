using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Drivers
{
    public class MarkdownBodyPartDisplay : ContentPartDisplayDriver<MarkdownBodyPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplatemanager;

        public MarkdownBodyPartDisplay(ILiquidTemplateManager liquidTemplatemanager, IStringLocalizer<MarkdownBodyPartDisplay> localizer)
        {
            _liquidTemplatemanager = liquidTemplatemanager;
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

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
                if (!string.IsNullOrEmpty(viewModel.Markdown) && !_liquidTemplatemanager.Validate(viewModel.Markdown, out var errors))
                {
                    context.Updater.ModelState.AddModelError(nameof(model.Markdown), T["The 'MarkdownBody' contains an invalid Liquid expression. Details: {0}", string.Join(" ", errors)]);
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
            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", MarkdownBodyPart.ContentItem);
            templateContext.MemberAccessStrategy.Register<MarkdownBodyPartViewModel>();

            model.MarkdownBodyPart = MarkdownBodyPart;
            model.ContentItem = MarkdownBodyPart.ContentItem;
            templateContext.LocalScope.SetValue("Model", model);

            var markdown = await _liquidTemplatemanager.RenderAsync(MarkdownBodyPart.Markdown, System.Text.Encodings.Web.HtmlEncoder.Default, templateContext);

            model.Markdown = MarkdownBodyPart.Markdown;
            model.Html = Markdig.Markdown.ToHtml(markdown ?? "");
        }
    }
}
