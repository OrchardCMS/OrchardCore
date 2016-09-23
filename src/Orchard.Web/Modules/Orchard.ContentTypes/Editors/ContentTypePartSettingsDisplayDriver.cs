using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentTypes.Editors
{
    public class ContentTypePartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
        {
            return Shape("ContentTypePartSettings_Edit", new { ContentPart = model }).Location("Content");
        }
    }
}