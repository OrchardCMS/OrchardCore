using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings;

public class MultiTextFieldPredefinedListEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MultiTextField>
{
    protected readonly IStringLocalizer S;

    public MultiTextFieldPredefinedListEditorSettingsDriver(IStringLocalizer<MultiTextFieldPredefinedListEditorSettingsDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
    {
        return Initialize<MultiTextFieldPredefinedListSettingsViewModel>("MultiTextFieldPredefinedListEditorSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<MultiTextFieldPredefinedListEditorSettings>();

            model.Options = JConvert.SerializeObject(settings.Options, JOptions.Indented);
        })
        .Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (partFieldDefinition.Editor() != "EditableList")
        {
            var model = new MultiTextFieldPredefinedListSettingsViewModel();
            var settings = new MultiTextFieldPredefinedListEditorSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            try
            {
                settings.Options = JConvert.DeserializeObject<MultiTextFieldValueOption[]>(model.Options);
            }
            catch
            {
                context.Updater.ModelState.AddModelError(Prefix, S["The options are written in an incorrect format."]);
                return Edit(partFieldDefinition);
            }

            context.Builder.WithSettings(settings);
        }

        return Edit(partFieldDefinition);
    }
}
