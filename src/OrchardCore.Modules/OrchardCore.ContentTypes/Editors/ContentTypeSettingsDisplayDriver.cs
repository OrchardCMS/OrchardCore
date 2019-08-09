using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors
{
    public class ContentTypeSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Initialize<ContentTypeSettingsViewModel>("ContentTypeSettings_Edit", model =>
            {
                var settings = contentTypeDefinition.Settings.ToObject<ContentTypeSettings>();

                model.Creatable = settings.Creatable;
                model.Listable = settings.Listable;
                model.Draftable = settings.Draftable;
                model.Versionable = settings.Versionable;
                model.Securable = settings.Securable;
                model.Stereotype = settings.Stereotype;
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
                context.Builder.Versionable(model.Versionable);
                context.Builder.Securable(model.Securable);
                context.Builder.Stereotype(model.Stereotype?.Trim());
            }

            return Edit(contentTypeDefinition, context.Updater);
        }
    }
}