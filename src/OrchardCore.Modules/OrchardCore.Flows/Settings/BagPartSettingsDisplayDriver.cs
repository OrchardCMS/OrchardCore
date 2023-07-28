using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Flows.Settings
{
    public class BagPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<BagPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly IStringLocalizer S;

        public BagPartSettingsDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<BagPartSettingsDisplayDriver> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<BagPartSettingsViewModel>("BagPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<BagPartSettings>();

                model.BagPartSettings = settings;
                model.ContainedContentTypes = model.BagPartSettings.ContainedContentTypes;
                model.DisplayType = model.BagPartSettings.DisplayType;
                model.ContentTypes = new NameValueCollection();
                model.Source = settings.ContainedStereotypes != null && settings.ContainedStereotypes.Length > 0 ? BagPartSettingType.Stereotypes : BagPartSettingType.ContentTypes;
                model.Stereotypes = String.Join(',', settings.ContainedStereotypes ?? Array.Empty<string>());
                foreach (var contentTypeDefinition in _contentDefinitionManager.ListTypeDefinitions())
                {
                    model.ContentTypes.Add(contentTypeDefinition.Name, contentTypeDefinition.DisplayName);
                }
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            var model = new BagPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ContainedContentTypes, m => m.DisplayType, m => m.Source, m => m.Stereotypes);

            switch (model.Source)
            {
                case BagPartSettingType.ContentTypes:
                    SetContentTypes(context, model);
                    break;
                case BagPartSettingType.Stereotypes:
                    SetStereoTypes(context, model);
                    break;
                default:
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Source), S["Content type source must be set with a valid value."]);
                    break;
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }

        private void SetStereoTypes(UpdateTypePartEditorContext context, BagPartSettingsViewModel model)
        {
            if (String.IsNullOrEmpty(model.Stereotypes))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Stereotypes), S["Please provide a Stereotype."]);

                return;
            }

            context.Builder.WithSettings(new BagPartSettings
            {
                ContainedContentTypes = Array.Empty<string>(),
                ContainedStereotypes = model.Stereotypes.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                DisplayType = model.DisplayType
            });
        }

        private void SetContentTypes(UpdateTypePartEditorContext context, BagPartSettingsViewModel model)
        {
            if (model.ContainedContentTypes == null || model.ContainedContentTypes.Length == 0)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.ContainedContentTypes), S["At least one content type must be selected."]);

                return;
            }

            context.Builder.WithSettings(new BagPartSettings
            {
                ContainedContentTypes = model.ContainedContentTypes,
                ContainedStereotypes = Array.Empty<string>(),
                DisplayType = model.DisplayType
            });
        }
    }
}
