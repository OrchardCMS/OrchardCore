using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ShortCodes.Services;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;

namespace OrchardCore.ContentFields.Drivers
{
    public class HtmlFieldDisplayDriver : ContentFieldDisplayDriver<HtmlField>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IShortCodeService _shortCodeService;
        private readonly IStringLocalizer S;

        public HtmlFieldDisplayDriver(ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder,
            IHtmlSanitizerService htmlSanitizerService,
            IShortCodeService shortCodeService,
            IStringLocalizer<HtmlFieldDisplayDriver> localizer)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
            _htmlSanitizerService = htmlSanitizerService;
            _shortCodeService = shortCodeService;
            S = localizer;
        }

        public override IDisplayResult Display(HtmlField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayHtmlFieldViewModel>(GetDisplayShapeType(context), async model =>
            {
                model.Html = field.Html;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;

                var settings = context.PartFieldDefinition.GetSettings<HtmlFieldSettings>();
                if (!settings.SanitizeHtml)
                {
                    model.Html = await _liquidTemplateManager.RenderAsync(field.Html, _htmlEncoder, model,
                        scope => scope.SetValue("ContentItem", field.ContentItem));
                }

                model.Html = await _shortCodeService.ProcessAsync(model.Html);

            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(HtmlField field, BuildFieldEditorContext context)
        {
            return Initialize<EditHtmlFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Html = field.Html;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(HtmlField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var viewModel = new EditHtmlFieldViewModel();

            var settings = context.PartFieldDefinition.GetSettings<HtmlFieldSettings>();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.Html))
            {
                if (!string.IsNullOrEmpty(viewModel.Html) && !_liquidTemplateManager.Validate(viewModel.Html, out var errors))
                {
                    var fieldName = context.PartFieldDefinition.DisplayName();
                    context.Updater.ModelState.AddModelError(nameof(field.Html), S["{0} doesn't contain a valid Liquid expression. Details: {1}", fieldName, string.Join(" ", errors)]);
                }
                else
                {
                    field.Html = settings.SanitizeHtml ? _htmlSanitizerService.Sanitize(viewModel.Html) : viewModel.Html;
                }
            }

            return Edit(field, context);
        }
    }
}
