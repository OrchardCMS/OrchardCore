using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Drivers
{
    public class MarkdownFieldDisplayDriver : ContentFieldDisplayDriver<MarkdownField>
    {
        private readonly ILiquidTemplateManager _liquidTemplatemanager;
        private readonly HtmlEncoder _htmlEncoder;

        public MarkdownFieldDisplayDriver(ILiquidTemplateManager liquidTemplatemanager, IStringLocalizer<MarkdownFieldDisplayDriver> localizer, HtmlEncoder htmlEncoder)
        {
            _liquidTemplatemanager = liquidTemplatemanager;
            T = localizer;
            _htmlEncoder = htmlEncoder;
        }

        public IStringLocalizer T { get; }

        public override IDisplayResult Display(MarkdownField field, BuildFieldDisplayContext context)
        {
            return Initialize<MarkdownFieldViewModel>(GetDisplayShapeType(context), async model =>
            {
                var templateContext = new TemplateContext();
                templateContext.SetValue("ContentItem", field.ContentItem);
                templateContext.MemberAccessStrategy.Register<MarkdownFieldViewModel>();

                model.Markdown = field.Markdown;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
                templateContext.SetValue("Model", model);

                model.Markdown = await _liquidTemplatemanager.RenderAsync(field.Markdown, _htmlEncoder, templateContext);
                model.Html = Markdig.Markdown.ToHtml(model.Markdown ?? "");
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(MarkdownField field, BuildFieldEditorContext context)
        {
            return Initialize<EditMarkdownFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Markdown = field.Markdown;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MarkdownField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var viewModel = new EditMarkdownFieldViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.Markdown))
            {
                if (!string.IsNullOrEmpty(viewModel.Markdown) && !_liquidTemplatemanager.Validate(viewModel.Markdown, out var errors))
                {
                    var fieldName = context.PartFieldDefinition.DisplayName();
                    context.Updater.ModelState.AddModelError(nameof(field.Markdown), T["{0} field doesn't contain a valid Liquid expression. Details: {1}", fieldName, string.Join(" ", errors)]);
                }
                else
                {
                    field.Markdown = viewModel.Markdown;
                }
            }

            return Edit(field, context);
        }
    }
}
