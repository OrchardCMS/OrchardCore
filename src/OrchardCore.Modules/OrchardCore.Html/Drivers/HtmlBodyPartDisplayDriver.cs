using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.Services;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using Shortcodes;

namespace OrchardCore.Html.Drivers;

public sealed class HtmlBodyPartDisplayDriver : ContentPartDisplayDriver<HtmlBodyPart>
{
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly IHtmlDisplayService _htmlDisplayService;

    internal readonly IStringLocalizer S;

    public HtmlBodyPartDisplayDriver(ILiquidTemplateManager liquidTemplateManager,
        IHtmlSanitizerService htmlSanitizerService,
        IHtmlDisplayService htmlDisplayService,
        IStringLocalizer<HtmlBodyPartDisplayDriver> localizer)
    {
        _liquidTemplateManager = liquidTemplateManager;
        _htmlSanitizerService = htmlSanitizerService;
        _htmlDisplayService = htmlDisplayService;
        S = localizer;
    }

    public override IDisplayResult Display(HtmlBodyPart htmlBodyPart, BuildPartDisplayContext context)
    {
        return Initialize<HtmlBodyPartViewModel, HtmlBodyPartDisplayDriver, HtmlBodyPart, BuildPartDisplayContext>(GetDisplayShapeType(context), static (m, driver, part, context) => driver.BuildViewModelAsync(m, part, context), this, htmlBodyPart, context)
            .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
            .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
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

    public override async Task<IDisplayResult> UpdateAsync(HtmlBodyPart model, UpdatePartEditorContext context)
    {
        var viewModel = new HtmlBodyPartViewModel();
        var settings = context.TypePartDefinition.GetSettings<HtmlBodyPartSettings>();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Html);

        model.Html = settings.SanitizeHtml
            ? _htmlSanitizerService.Sanitize(viewModel.Html)
            : viewModel.Html;

        if (settings.RenderLiquid
            && !string.IsNullOrEmpty(model.Html)
            && !_liquidTemplateManager.Validate(model.Html, out var errors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Html),
                S[settings.SanitizeHtml
                    ? "{0} contains invalid Liquid expression. Note that HTML sanitization affects the value being saved and thus can break Liquid code: {1}"
                    : "{0} contains invalid Liquid expression: {1}",
                    context.TypePartDefinition.DisplayName(),
                    string.Join(" ", errors)]);
        }

        return Edit(model, context);
    }

    private async ValueTask BuildViewModelAsync(HtmlBodyPartViewModel model, HtmlBodyPart htmlBodyPart, BuildPartDisplayContext context)
    {
        model.Html = htmlBodyPart.Html;
        model.HtmlBodyPart = htmlBodyPart;
        model.ContentItem = htmlBodyPart.ContentItem;

        var settings = context.TypePartDefinition.GetSettings<HtmlBodyPartSettings>();

        await _htmlDisplayService.UpdateModelHtmlAsync(
            model,
            settings.RenderLiquid,
            new Context { ["TypePartDefinition"] = context.TypePartDefinition },
            settings.SanitizeHtml);
    }
}
