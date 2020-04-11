using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors
{
    public class ContentPartSettingsDisplayDriver : ContentPartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        {
            return Initialize<ContentPartSettingsViewModel>("ContentPartSettings_Edit", model =>
            {
                var settings = contentPartDefinition.GetSettings<ContentPartSettings>();

                model.Attachable = settings.Attachable;
                model.Reusable = settings.Reusable;
                model.Description = settings.Description;
                model.DisplayName = settings.DisplayName;
                model.ContentPartDefinition = contentPartDefinition;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartDefinition contentPartDefinition, UpdatePartEditorContext context)
        {
            var model = new ContentPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.Attachable(model.Attachable);
                context.Builder.Reusable(model.Reusable);
                context.Builder.WithDescription(model.Description);
                context.Builder.WithDisplayName(model.DisplayName);
            }

            return Edit(contentPartDefinition);
        }
    }
}
