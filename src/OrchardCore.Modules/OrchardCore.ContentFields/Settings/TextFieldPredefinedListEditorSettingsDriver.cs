using System.Text.Json;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings;

public sealed class TextFieldPredefinedListEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<TextField>
{
    internal readonly IStringLocalizer S;

    public TextFieldPredefinedListEditorSettingsDriver(IStringLocalizer<TextFieldPredefinedListEditorSettingsDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<PredefinedListSettingsViewModel>("TextFieldPredefinedListEditorSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<TextFieldPredefinedListEditorSettings>();

            model.DefaultValue = settings.DefaultValue;
            model.Editor = settings.Editor;
            model.Options = JConvert.SerializeObject(settings.Options ?? [], JOptions.Indented);
        }).Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (partFieldDefinition.Editor() == "PredefinedList")
        {
            var model = new PredefinedListSettingsViewModel();
            var settings = new TextFieldPredefinedListEditorSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            try
            {
                settings.DefaultValue = model.DefaultValue;
                settings.Editor = model.Editor;
                settings.Options = string.IsNullOrWhiteSpace(model.Options)
                    ? []
                    : JConvert.DeserializeObject<ListValueOption[]>(model.Options);

                context.Builder.WithSettings(settings);
            }
            catch
            {
                context.Updater.ModelState.AddModelError(Prefix, S["The options are written in an incorrect format."]);
            }
        }

        return Edit(partFieldDefinition, context);
    }
}
