using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Services;
using Shortcodes;

namespace OrchardCore.ContentFields.Drivers;

public sealed class HtmlFieldDisplayDriver : ContentFieldDisplayDriver<HtmlField>
{
    private readonly IHtmlDisplayService _htmlDisplayService;

    public HtmlFieldDisplayDriver(IHtmlDisplayService htmlDisplayService)
    {
        _htmlDisplayService = htmlDisplayService;
    }

    public override IDisplayResult Display(HtmlField field, BuildFieldDisplayContext context)
    {
        return Initialize<DisplayHtmlFieldViewModel, HtmlFieldDisplayDriver, HtmlField, BuildFieldDisplayContext>(GetDisplayShapeType(context), static async (model, driver, field, context) =>
        {
            model.Html = field.Html;
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
            model.ContentItem = field.ContentItem;

            var settings = context.PartFieldDefinition.GetSettings<HtmlFieldSettings>();

            await driver._htmlDisplayService.UpdateModelHtmlAsync(
                model,
                new Context { ["PartFieldDefinition"] = context.PartFieldDefinition },
                settings.SanitizeHtml);
        }, this, field, context)
        .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
        .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
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

    public override async Task<IDisplayResult> UpdateAsync(HtmlField field, UpdateFieldEditorContext context)
    {
        var viewModel = new EditHtmlFieldViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, f => f.Html);

        field.Html = viewModel.Html;

        return Edit(field, context);
    }
}
