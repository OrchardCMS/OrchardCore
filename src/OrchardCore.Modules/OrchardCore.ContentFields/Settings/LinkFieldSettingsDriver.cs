using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings;

public sealed class LinkFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<LinkField>
{
    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<LinkFieldSettings>("LinkFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<LinkFieldSettings>();

            model.Hint = settings.Hint;
            model.HintLinkText = settings.HintLinkText;
            model.Required = settings.Required;
            model.LinkTextMode = settings.LinkTextMode;
            model.UrlPlaceholder = settings.UrlPlaceholder;
            model.TextPlaceholder = settings.TextPlaceholder;
            model.DefaultUrl = settings.DefaultUrl;
            model.DefaultText = settings.DefaultText;
            model.DefaultTarget = settings.DefaultTarget;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new LinkFieldSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model);

        return Edit(partFieldDefinition, context);
    }
}
