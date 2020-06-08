using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors
{
    public class ContentTypePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
        {
            return Shape("ContentTypePartSettings_Edit", new ShapeViewModel<ContentTypePartDefinition>(model)).Location("Content");
        }
    }
}
