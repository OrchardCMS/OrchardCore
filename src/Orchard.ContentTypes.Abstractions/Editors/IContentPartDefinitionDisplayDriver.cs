using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentTypes.Editors
{
    public interface IContentPartDefinitionDisplayDriver : IDisplayDriver<ContentPartDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartEditorContext>
    {
    }
}