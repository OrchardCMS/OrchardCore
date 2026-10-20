using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.Services;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.Mvc.ModelBinding;
using Shortcodes;

namespace OrchardCore.Html.Drivers;

public sealed class HtmlBodyPartDisplayDriver : ContentPartDisplayDriver<HtmlBodyPart>
{
    private readonly IHtmlDisplayService _htmlDisplayService;

    public HtmlBodyPartDisplayDriver(IHtmlDisplayService htmlDisplayService)
    {
        _htmlDisplayService = htmlDisplayService;
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
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Html);

        model.Html = viewModel.Html;

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
            new Context { ["TypePartDefinition"] = context.TypePartDefinition },
            settings.SanitizeHtml);
    }
}
