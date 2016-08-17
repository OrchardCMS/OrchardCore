using System;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Body.Settings
{
    public class BodyPartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
        {
            if (!String.Equals("BodyPart", model.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape("BodyPartSettings_Edit", new { ContentPart = model }).Location("Content");
        }
    }
}