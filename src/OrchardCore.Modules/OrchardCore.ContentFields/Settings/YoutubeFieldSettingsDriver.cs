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
                 partFieldDefinition.Settings.Populate(model);
                 model.Height = model.Height != default(int) ? model.Height : 315;
                 model.Width = model.Width != default(int) ? model.Width : 560;

             }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new YoutubeFieldSettings();
            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.MergeSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
