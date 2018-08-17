using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentTypes.Editors
{
    public interface IContentPartFieldEditorSettingsDisplayDriver : IDisplayDriver<ContentPartFieldDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartFieldEditorContext>
    {
    }
}