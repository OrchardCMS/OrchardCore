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
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Drivers
{
    public class MarkdownFieldDisplayDriver : ContentFieldDisplayDriver<MarkdownField>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IShortCodeService _shortCodeService;
        private readonly IMarkdownService _markdownService;
        private readonly IStringLocalizer S;

        public MarkdownFieldDisplayDriver(ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder,
            IHtmlSanitizerService htmlSanitizerService,
            IShortCodeService shortCodeService,
            IMarkdownService markdownService,
            IStringLocalizer<MarkdownFieldDisplayDriver> localizer)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
            _htmlSanitizerService = htmlSanitizerService;
            _shortCodeService = shortCodeService;
            _markdownService = markdownService;
            S = localizer;
        }

        public override IDisplayResult Display(MarkdownField field, BuildFieldDisplayContext context)
        {
            return Initialize<MarkdownFieldViewModel>(GetDisplayShapeType(context), async model =>
            {

                var settings = context.PartFieldDefinition.GetSettings<MarkdownFieldSettings>();
                model.Markdown = field.Markdown;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;

                // The default Markdown option is to entity escape html
                // so filters must be run after the markdown has been processed.
                model.Html = _markdownService.ToHtml(model.Markdown ?? "");

                // The liquid rendering is for backwards compatability and can be removed in a future version.
                if (!settings.SanitizeHtml)
                {
                    model.Markdown = await _liquidTemplateManager.RenderAsync(model.Html, _htmlEncoder, model,
                        scope => scope.SetValue("ContentItem", field.ContentItem));
                }

                model.Html = await _shortCodeService.ProcessAsync(model.Html ?? "");

                if (settings.SanitizeHtml)
                {
                    model.Html = _htmlSanitizerService.Sanitize(model.Html ?? "");
                }
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
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
                if (!string.IsNullOrEmpty(viewModel.Markdown) && !_liquidTemplateManager.Validate(viewModel.Markdown, out var errors))
                {
                    var fieldName = context.PartFieldDefinition.DisplayName();
                    context.Updater.ModelState.AddModelError(nameof(field.Markdown), S["{0} field doesn't contain a valid Liquid expression. Details: {1}", fieldName, string.Join(" ", errors)]);
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
