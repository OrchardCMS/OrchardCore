using System;
using System.Threading.Tasks;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.ViewModels;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Autoroute.Settings
{
    public class AutoroutePartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(AutoroutePart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<AutoroutePartSettingsViewModel>("AutoroutePartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.Settings.ToObject<AutoroutePartSettings>();

                model.AllowCustomPath = settings.AllowCustomPath;
                model.Pattern = settings.Pattern;
                model.ShowHomepageOption = settings.ShowHomepageOption;
                model.AutoroutePartSettings = settings;

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(AutoroutePart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new AutoroutePartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, 
                m => m.Pattern, 
                m => m.AllowCustomPath, 
                m => m.ShowHomepageOption);

            context.Builder.WithSetting(nameof(AutoroutePartSettings.Pattern), model.Pattern);
            context.Builder.WithSetting(nameof(AutoroutePartSettings.AllowCustomPath), model.AllowCustomPath.ToString());
            context.Builder.WithSetting(nameof(AutoroutePartSettings.ShowHomepageOption), model.ShowHomepageOption.ToString());

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}