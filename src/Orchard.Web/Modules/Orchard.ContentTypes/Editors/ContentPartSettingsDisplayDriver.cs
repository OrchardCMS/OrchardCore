using System.Threading.Tasks;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.ViewModels;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentTypes.Editors
{
    public class ContentPartSettingsDisplayDriver : ContentPartDisplayDriver
    {

        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        {
            return Shape<ContentPartSettingsViewModel>("ContentPartSettings_Edit", model =>
            {
                var settings = contentPartDefinition.Settings.ToObject<ContentPartSettings>();

                model.Attachable = settings.Attachable;
                model.Reusable = settings.Reusable;
                model.Description = settings.Description;

                return Task.CompletedTask;
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
            }

            return Edit(contentPartDefinition, context.Updater);
        }
    }
}