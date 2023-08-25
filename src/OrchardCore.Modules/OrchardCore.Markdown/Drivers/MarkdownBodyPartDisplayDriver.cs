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
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Markdown.Drivers
{
    public class MarkdownBodyPartDisplayDriver : ContentPartDisplayDriver<MarkdownBodyPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IShortcodeService _shortcodeService;
        private readonly IMarkdownService _markdownService;
        protected readonly IStringLocalizer S;

        public MarkdownBodyPartDisplayDriver(ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder,
            IHtmlSanitizerService htmlSanitizerService,
            IShortcodeService shortcodeService,
            IMarkdownService markdownService,
            IStringLocalizer<MarkdownBodyPartDisplayDriver> localizer)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
            _htmlSanitizerService = htmlSanitizerService;
            _shortcodeService = shortcodeService;
            _markdownService = markdownService;
            S = localizer;
        }

        public override IDisplayResult Display(MarkdownBodyPart markdownBodyPart, BuildPartDisplayContext context)
        {
            return Initialize<MarkdownBodyPartViewModel>(GetDisplayShapeType(context), m => BuildViewModel(m, markdownBodyPart, context))
                .Location("Detail", "Content")
                .Location("Summary", "Content");
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

            if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix, vm => vm.Markdown))
            {
                if (!String.IsNullOrEmpty(viewModel.Markdown) && !_liquidTemplateManager.Validate(viewModel.Markdown, out var errors))
                {
                    var partName = context.TypePartDefinition.DisplayName();
                    updater.ModelState.AddModelError(Prefix, nameof(viewModel.Markdown), S["{0} doesn't contain a valid Liquid expression. Details: {1}", partName, String.Join(" ", errors)]);
                }
                else
                {
                    model.Markdown = viewModel.Markdown;
                }
            }

            return Edit(model, context);
        }

        private async ValueTask BuildViewModel(MarkdownBodyPartViewModel model, MarkdownBodyPart markdownBodyPart, BuildPartDisplayContext context)
        {
            model.Markdown = markdownBodyPart.Markdown;
            model.MarkdownBodyPart = markdownBodyPart;
            model.ContentItem = markdownBodyPart.ContentItem;

            // The default Markdown option is to entity escape html
            // so filters must be run after the markdown has been processed.
            model.Html = _markdownService.ToHtml(model.Markdown ?? "");

            var settings = context.TypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

            // The liquid rendering is for backwards compatibility and can be removed in a future version.
            if (!settings.SanitizeHtml)
            {
                model.Html = await _liquidTemplateManager.RenderStringAsync(model.Html, _htmlEncoder, model,
                    new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });
            }

            model.Html = await _shortcodeService.ProcessAsync(model.Html,
                new Context
                {
                    ["ContentItem"] = markdownBodyPart.ContentItem,
                    ["TypePartDefinition"] = context.TypePartDefinition
                });

            if (settings.SanitizeHtml)
            {
                model.Html = _htmlSanitizerService.Sanitize(model.Html ?? "");
            }
        }
    }
}
