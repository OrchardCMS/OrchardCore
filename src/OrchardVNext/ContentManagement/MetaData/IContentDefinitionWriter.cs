using System.Xml.Linq;
using OrchardVNext.ContentManagement.MetaData.Models;

namespace OrchardVNext.ContentManagement.MetaData {
    public interface IContentDefinitionWriter : IDependency{
        XElement Export(ContentTypeDefinition typeDefinition);
        XElement Export(ContentPartDefinition partDefinition);
    }
}
