using System.Xml.Linq;
using OrchardVNext.ContentManagement.MetaData.Models;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.ContentManagement.MetaData {
    public interface IContentDefinitionWriter : IDependency{
        XElement Export(ContentTypeDefinition typeDefinition);
        XElement Export(ContentPartDefinition partDefinition);
    }
}
