using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.ViewModels;

namespace OrchardCore.Html.Settings;

public sealed class HtmlBodyPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<HtmlBodyPart>
{
    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<HtmlBodyPartSettingsViewModel>("HtmlBodyPartSettings_Edit", model =>
        {
            var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartSettings>();

            model.SanitizeHtml = settings.SanitizeHtml;
        }).Location("Content:20");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        var model = new HtmlBodyPartSettingsViewModel();
        var settings = new HtmlBodyPartSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.SanitizeHtml = model.SanitizeHtml;

        context.Builder.WithSettings(settings);

        return Edit(contentTypePartDefinition, context);
    }
}
