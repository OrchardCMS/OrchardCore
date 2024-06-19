using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings;

public class MultiTextFieldEditableListEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MultiTextField>
{
    protected readonly IStringLocalizer S;

    public MultiTextFieldEditableListEditorSettingsDriver(IStringLocalizer<MultiTextFieldEditableListEditorSettingsDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
    {
        return Initialize<MultiTextFieldEditableListSettingsViewModel>("MultiTextFieldEditableListEditorSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<MultiTextFieldEditableListEditorSettings>();

            model.Items = JConvert.SerializeObject(settings.Items, JOptions.Indented);
        })
        .Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (partFieldDefinition.Editor() == "EditableList")
        {
            var model = new MultiTextFieldEditableListSettingsViewModel();
            var settings = new MultiTextFieldEditableListEditorSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            try
            {
                settings.Items = JConvert.DeserializeObject<string[]>(model.Items);
            }
            catch
            {
                context.Updater.ModelState.AddModelError(Prefix, S["The items are written in an incorrect format."]);
                return Edit(partFieldDefinition);
            }

            context.Builder.WithSettings(settings);
        }

        return Edit(partFieldDefinition);
    }
}
