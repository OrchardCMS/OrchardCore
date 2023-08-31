using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Html.Drivers
{
    public class HtmlBodyPartDisplayDriver : ContentPartDisplayDriver<HtmlBodyPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IShortcodeService _shortcodeService;
        protected readonly IStringLocalizer S;

        public HtmlBodyPartDisplayDriver(ILiquidTemplateManager liquidTemplateManager,
            IHtmlSanitizerService htmlSanitizerService,
            HtmlEncoder htmlEncoder,
            IShortcodeService shortcodeService,
            IStringLocalizer<HtmlBodyPartDisplayDriver> localizer)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlSanitizerService = htmlSanitizerService;
            _htmlEncoder = htmlEncoder;
            _shortcodeService = shortcodeService;
            S = localizer;
        }

        public override IDisplayResult Display(HtmlBodyPart HtmlBodyPart, BuildPartDisplayContext context)
        {
            return Initialize<HtmlBodyPartViewModel>(GetDisplayShapeType(context), m => BuildViewModelAsync(m, HtmlBodyPart, context))
                .Location("Detail", "Content")
                .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(HtmlBodyPart HtmlBodyPart, BuildPartEditorContext context)
        {
            return Initialize<HtmlBodyPartViewModel>(GetEditorShapeType(context), model =>
            {
                model.Html = HtmlBodyPart.Html;
                model.ContentItem = HtmlBodyPart.ContentItem;
                model.HtmlBodyPart = HtmlBodyPart;
                model.TypePartDefinition = context.TypePartDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(HtmlBodyPart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new HtmlBodyPartViewModel();

            var settings = context.TypePartDefinition.GetSettings<HtmlBodyPartSettings>();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Html))
            {
                if (!String.IsNullOrEmpty(viewModel.Html) && !_liquidTemplateManager.Validate(viewModel.Html, out var errors))
                {
                    var partName = context.TypePartDefinition.DisplayName();
                    updater.ModelState.AddModelError(Prefix, nameof(viewModel.Html), S["{0} doesn't contain a valid Liquid expression. Details: {1}", partName, String.Join(" ", errors)]);
                }
                else
                {
                    model.Html = settings.SanitizeHtml ? _htmlSanitizerService.Sanitize(viewModel.Html) : viewModel.Html;
                }
            }

            return Edit(model, context);
        }

        private async ValueTask BuildViewModelAsync(HtmlBodyPartViewModel model, HtmlBodyPart htmlBodyPart, BuildPartDisplayContext context)
        {
            model.Html = htmlBodyPart.Html;
            model.HtmlBodyPart = htmlBodyPart;
            model.ContentItem = htmlBodyPart.ContentItem;
            var settings = context.TypePartDefinition.GetSettings<HtmlBodyPartSettings>();

            if (!settings.SanitizeHtml)
            {
                model.Html = await _liquidTemplateManager.RenderStringAsync(htmlBodyPart.Html, _htmlEncoder, model,
                    new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });
            }

            model.Html = await _shortcodeService.ProcessAsync(model.Html,
                new Context
                {
                    ["ContentItem"] = htmlBodyPart.ContentItem,
                    ["TypePartDefinition"] = context.TypePartDefinition
                });
        }
    }
}
