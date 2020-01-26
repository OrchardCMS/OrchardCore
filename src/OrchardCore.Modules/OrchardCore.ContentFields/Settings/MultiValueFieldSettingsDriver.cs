using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class MultiValueFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MultiValueField>
    {
        private readonly IStringLocalizer S;

        public MultiValueFieldSettingsDriver(IStringLocalizer<MultiValueFieldSettingsDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<MultiValueFieldSettings>("MultiValueFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<MultiValueFieldSettings>();

                model.Required = settings.Required;
                model.Hint = settings.Hint;
                model.DefaultValue = settings.DefaultValue;
                model.Editor = settings.Editor;
                model.Options = settings.Options ?? JsonConvert.SerializeObject(new MultiValueListValueOption[0], Formatting.Indented);
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
                var model = new MultiValueSettingsViewModel();
                var settings = new MultiValueFieldSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                try
                {
                    settings.Required = model.Required;
                    settings.Hint = model.Hint;
                    settings.DefaultValue = model.DefaultValue;
                    settings.Editor = model.Editor;
                    settings.Options = model.Options; // string.IsNullOrWhiteSpace(model.Options)
                        //? new MultiValueListValueOption[0]
                        //: JsonConvert.DeserializeObject<MultiValueListValueOption[]>(model.Options);
                }
                catch
                {
                    context.Updater.ModelState.AddModelError(Prefix, S["The options are written in an incorrect format."]);
                    return Edit(partFieldDefinition);
                }

                context.Builder.WithSettings(settings);

            return Edit(partFieldDefinition);
        }
    }
}
