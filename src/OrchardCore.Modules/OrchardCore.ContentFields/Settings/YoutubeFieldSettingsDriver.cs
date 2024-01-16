using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class YoutubeFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<YoutubeField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<YoutubeFieldSettings>("YoutubeFieldSetting_Edit", model =>
             {
                 partFieldDefinition.PopulateSettings(model);
                 model.Height = model.Height != default ? model.Height : 315;
                 model.Width = model.Width != default ? model.Width : 560;
             }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new YoutubeFieldSettings();
            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
