using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Lists.Settings
{
    public class CommonPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<CommonPart>
    {
        public override Task<IDisplayResult> EditAsync(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<CommonPartSettingsViewModel>("CommonPartSettings_Edit", model =>
                {
                    var settings = contentTypePartDefinition.GetSettings<CommonPartSettings>();
                    model.DisplayDateEditor = settings.DisplayDateEditor;
                    model.DisplayOwnerEditor = settings.DisplayOwnerEditor;
                }).Location("Content")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            var model = new CommonPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(new CommonPartSettings
            {
                DisplayDateEditor = model.DisplayDateEditor,
                DisplayOwnerEditor = model.DisplayOwnerEditor,
            });

            return await EditAsync(contentTypePartDefinition, context);
        }
    }
}
