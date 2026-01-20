using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors;

public sealed class ContentTypePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    public override IDisplayResult Edit(ContentTypePartDefinition model, BuildEditorContext context)
    {
        return Shape("ContentTypePartSettings_Edit", new ShapeViewModel<ContentTypePartDefinition>(model)).Location("Content");
    }
}
