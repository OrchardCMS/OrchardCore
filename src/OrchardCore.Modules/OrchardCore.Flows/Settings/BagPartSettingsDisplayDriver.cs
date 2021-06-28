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

namespace OrchardCore.Flows.Settings
{
    public class BagPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<BagPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IStringLocalizer S;

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
                model.BagPartSettings = contentTypePartDefinition.GetSettings<BagPartSettings>();
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
            var model = new BagPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ContainedContentTypes, m => m.DisplayType);

            if (model.ContainedContentTypes == null || model.ContainedContentTypes.Length == 0)
            {
                context.Updater.ModelState.AddModelError(nameof(model.ContainedContentTypes), S["At least one content type must be selected."]);
            }
            else
            {
                context.Builder.WithSettings(new BagPartSettings
                {
                    ContainedContentTypes = model.ContainedContentTypes,
                    DisplayType = model.DisplayType
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
