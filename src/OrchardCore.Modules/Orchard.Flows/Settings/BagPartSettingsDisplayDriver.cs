using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Flows.Models;
using Orchard.Flows.ViewModels;

namespace Orchard.Flows.Settings
{
    public class BagPartSettingsDisplayDriver : ContentTypePartDisplayDriver
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

            return Shape<BagPartSettingsViewModel>("BagPartSettings_Edit", model =>
            {
                model.BagPartSettings = contentTypePartDefinition.Settings.ToObject<BagPartSettings>();
                model.ContainedContentTypes = model.BagPartSettings.ContainedContentTypes;
                model.ContentTypes = new NameValueCollection();

                foreach(var contentTypeDefinition in _contentDefinitionManager.ListTypeDefinitions())
                {
                    model.ContentTypes.Add(contentTypeDefinition.Name, contentTypeDefinition.DisplayName);
                }

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(BagPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new BagPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ContainedContentTypes);

            if (model.ContainedContentTypes == null || model.ContainedContentTypes.Length == 0)
            {
                context.Updater.ModelState.AddModelError(nameof(model.ContainedContentTypes), S["At least one content type must be selected."]);
            }
            else
            {
                context.Builder.ContainedContentTypes(model.ContainedContentTypes);
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}