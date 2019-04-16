using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Facebook.Widgets.Models;

namespace OrchardCore.Facebook.Widgets.Settings
{
    public class FacebookPluginPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly IStringLocalizer<FacebookPluginPartSettingsDisplayDriver> T;

        public FacebookPluginPartSettingsDisplayDriver(IStringLocalizer<FacebookPluginPartSettingsDisplayDriver> localizer)
        {
            T = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(FacebookPluginPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<FacebookPluginPartSettingsViewModel>("FacebookPluginPartSettings_Edit", model =>
            {
                model.FacebookPluginPartSettings = contentTypePartDefinition.GetSettings<FacebookPluginPartSettings>();
                model.Liquid = model.FacebookPluginPartSettings.Liquid;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(FacebookPluginPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new FacebookPluginPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.Liquid);

            if (context.Updater.ModelState.ValidationState == ModelValidationState.Valid)
            {
                model.FacebookPluginPartSettings = new FacebookPluginPartSettings()
                {
                    Liquid = model.Liquid
                };
                context.Builder.WithSettings(model.FacebookPluginPartSettings);
            }
            return Edit(contentTypePartDefinition, context.Updater);
        }

    }
}