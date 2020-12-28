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
    public class MultiTextFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MultiTextField>
    {
        private readonly IStringLocalizer S;

        public MultiTextFieldSettingsDriver(IStringLocalizer<MultiTextFieldSettingsDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<MultiTextFieldSettings>("MultiTextFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<MultiTextFieldSettings>();

                model.Required = settings.Required;
                model.Hint = settings.Hint;
                model.Options = settings.Options ?? JsonConvert.SerializeObject(new MultiTextListValueOption[0], Formatting.Indented);
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
                var model = new MultiTextSettingsViewModel();
                var settings = new MultiTextFieldSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                try
                {
                    settings.Required = model.Required;
                    settings.Hint = model.Hint;
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
