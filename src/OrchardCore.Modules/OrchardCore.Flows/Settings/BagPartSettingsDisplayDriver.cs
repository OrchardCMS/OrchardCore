using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;

namespace OrchardCore.Flows.Settings
{
    public class BagPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public BagPartSettingsDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<BagPartSettingsDisplayDriver> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            S = localizer;
        }

        public IStringLocalizer S { get; set; }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(BagPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<BagPartSettingsViewModel>("BagPartSettings_Edit", model =>
            {
                model.BagPartSettings = contentTypePartDefinition.Settings.ToObject<BagPartSettings>();
                model.ContainedContentTypes = model.BagPartSettings.ContainedContentTypes;
                model.DisplayType = model.BagPartSettings.DisplayType;
                model.ContentTypes = new NameValueCollection();

                foreach (var contentTypeDefinition in _contentDefinitionManager.ListTypeDefinitions())
                {
                    model.ContentTypes.Add(contentTypeDefinition.Name, contentTypeDefinition.DisplayName);
                }
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(BagPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new BagPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ContainedContentTypes, m => m.DisplayType);

            if (model.ContainedContentTypes == null || model.ContainedContentTypes.Length == 0)
            {
                context.Updater.ModelState.AddModelError(nameof(model.ContainedContentTypes), S["At least one content type must be selected."]);
            }
            else
            {
                context.Builder.WithSetting(nameof(BagPartSettings.ContainedContentTypes), model.ContainedContentTypes);
                context.Builder.WithSetting(nameof(BagPartSettings.DisplayType), model.DisplayType);
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}