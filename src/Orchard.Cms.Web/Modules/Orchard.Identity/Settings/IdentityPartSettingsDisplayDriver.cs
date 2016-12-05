using System;
using System.Threading.Tasks;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Identity.Models;

namespace Orchard.Identity.Settings
{
    public class IdentityPartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(IdentityPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<IdentityPartSettingsViewModel>("IdentityPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<IdentityPartSettings>();

                model.AllowChangeIdentity = settings.AllowChangeIdentity;
                model.AllowCustomIdentity = settings.AllowCustomIdentity;
                model.ShowIdentityEditor = settings.ShowIdentityEditor;
                model.ShowNameEditor = settings.ShowNameEditor;
                model.IdentityPartSettings = settings;

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(IdentityPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new IdentityPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.AllowCustomIdentity,
                m => m.AllowChangeIdentity,
                m => m.ShowNameEditor,
                m => m.ShowIdentityEditor))
            {
                context.Builder.WithSettings(new IdentityPartSettings {
                    AllowCustomIdentity = model.AllowCustomIdentity,
                    ShowIdentityEditor = model.ShowIdentityEditor,
                    ShowNameEditor = model.ShowNameEditor,
                    AllowChangeIdentity = model.AllowChangeIdentity
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}