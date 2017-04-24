using System.Threading.Tasks;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.ViewModels;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentTypes.Editors
{
    public class ContentTypeSettingsDisplayDriver : ContentTypeDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Shape<ContentTypeSettingsViewModel>("ContentTypeSettings_Edit", model =>
            {
                var settings = contentTypeDefinition.Settings.ToObject<ContentTypeSettings>();

                model.Creatable = settings.Creatable;
                model.Listable = settings.Listable;
                model.Draftable = settings.Draftable;
                model.Updatable = settings.Updatable;
                model.Securable = settings.Securable;
                model.Stereotype = settings.Stereotype;

                return Task.CompletedTask;
            }).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
        {
            var model = new ContentTypeSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.Creatable(model.Creatable);
                context.Builder.Listable(model.Listable);
                context.Builder.Draftable(model.Draftable);
                context.Builder.Updatable(model.Updatable);
                context.Builder.Securable(model.Securable);
                context.Builder.Stereotype(model.Stereotype);
            }

            return Edit(contentTypeDefinition, context.Updater);
        }
    }
}