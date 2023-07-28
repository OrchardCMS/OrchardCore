using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.ContentFields.Drivers
{
    public class HtmlFieldDisplayDriver : ContentFieldDisplayDriver<HtmlField>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IShortcodeService _shortcodeService;
        protected readonly IStringLocalizer S;

        public HtmlFieldDisplayDriver(ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder,
            IHtmlSanitizerService htmlSanitizerService,
            IShortcodeService shortcodeService,
            IStringLocalizer<HtmlFieldDisplayDriver> localizer)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
            _htmlSanitizerService = htmlSanitizerService;
            _shortcodeService = shortcodeService;
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
                    model.Html = await _liquidTemplateManager.RenderStringAsync(field.Html, _htmlEncoder, model,
                        new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(field.ContentItem) });
                }

                model.Html = await _shortcodeService.ProcessAsync(model.Html,
                    new Context
                    {
                        ["ContentItem"] = field.ContentItem,
                        ["PartFieldDefinition"] = context.PartFieldDefinition
                    });

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
                if (!String.IsNullOrEmpty(viewModel.Html) && !_liquidTemplateManager.Validate(viewModel.Html, out var errors))
                {
                    var fieldName = context.PartFieldDefinition.DisplayName();
                    context.Updater.ModelState.AddModelError(
                        Prefix,
                        nameof(viewModel.Html), S["{0} doesn't contain a valid Liquid expression. Details: {1}",
                        fieldName,
                        String.Join(' ', errors)]);
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
