using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;

namespace OrchardCore.Flows.Settings
{
    public class FlowPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<FlowPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IStringLocalizer S;

        public FlowPartSettingsDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<FlowPartSettingsDisplayDriver> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<FlowPartSettingsViewModel>("FlowPartSettings_Edit", model =>
            {
                model.FlowPartSettings = contentTypePartDefinition.GetSettings<FlowPartSettings>();
                model.ContainedContentTypes = model.FlowPartSettings.ContainedContentTypes;
                model.ContentTypes = new NameValueCollection();

                foreach (var contentTypeDefinition in _contentDefinitionManager.ListTypeDefinitions().Where(t => t.GetSettings<ContentTypeSettings>().Stereotype == "Widget"))
                {
                    model.ContentTypes.Add(contentTypeDefinition.Name, contentTypeDefinition.DisplayName);
                }
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            var model = new FlowPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ContainedContentTypes);

            context.Builder.WithSettings(new FlowPartSettings
            {
                ContainedContentTypes = model.ContainedContentTypes
            });

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
