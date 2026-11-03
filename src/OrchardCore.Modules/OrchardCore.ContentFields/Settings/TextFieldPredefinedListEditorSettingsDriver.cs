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

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            ListValueOption[] options = null;
            var isValid = true;

            try
            {
                options = string.IsNullOrWhiteSpace(model.Options)
                    ? []
                    : JConvert.DeserializeObject<ListValueOption[]>(model.Options);
            }
            catch
            {
                isValid = false;
                context.Updater.ModelState.AddModelError(Prefix, S["The options are written in an incorrect format."]);
            }

            if (options != null)
            {
                // PR #19581 review requirement #3 ("adding a new option with the same label and
                // value as a different option should not silently coexist as an ambiguous
                // duplicate"): the client-side editor's auto-fill (see options-table-editor.ts's
                // OptionsTableAutoFillColumn) makes rows sharing a label very likely to also
                // share a value if neither has been directly edited, so this is now easy to
                // create by accident - flag it as a validation error rather than silently
                // persisting a duplicate.
                if (options.DistinctBy(o => $"{o.Name},{o.Value}").Count() != options.Length)
                {
                    isValid = false;
                    context.Updater.ModelState.AddModelError(Prefix, S["The options can't contain more than one element with the same label and value."]);
                }

                if (isValid)
                {
                    context.Builder.WithSettings(new TextFieldPredefinedListEditorSettings
                    {
                        DefaultValue = model.DefaultValue,
                        Editor = model.Editor,
                        Options = options,
                    });
                }
            }

            // Re-render from the posted model values, not from re-reading the (possibly stale,
            // previously-persisted) settings via Edit(partFieldDefinition, ...): on a validation
            // error the admin's in-progress edits would otherwise be discarded and replaced by
            // whatever was last saved.
            return Initialize<PredefinedListSettingsViewModel>("TextFieldPredefinedListEditorSettings_Edit", m =>
            {
                m.DefaultValue = model.DefaultValue;
                m.Editor = model.Editor;
                m.Options = model.Options;
            }).Location("Editor");
        }

        return Edit(partFieldDefinition, context);
    }
}
