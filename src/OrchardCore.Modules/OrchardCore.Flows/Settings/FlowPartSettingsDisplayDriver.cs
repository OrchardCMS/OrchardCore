using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
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

        public FlowPartSettingsDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<FlowPartSettingsViewModel>("FlowPartSettings_Edit", model =>
            {
                model.FlowPartSettings = contentTypePartDefinition.GetSettings<FlowPartSettings>();
                model.ContainedContentTypes = model.FlowPartSettings.ContainedContentTypes;
                model.ContentTypes = new NameValueCollection();

                foreach (var contentTypeDefinition in _contentDefinitionManager.ListTypeDefinitions().Where(t => t.GetStereotype() == "Widget"))
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
