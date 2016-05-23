using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentTypes.Editors
{
    public interface IContentPartDefinitionDisplayDriver : IDisplayDriver<ContentPartDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartEditorContext>, IDependency
    {
    }
}