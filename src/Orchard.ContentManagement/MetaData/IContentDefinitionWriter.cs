using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;

namespace Orchard.ContentManagement.MetaData
{
    public interface IContentDefinitionWriter : IDependency
    {
        XElement Export(ContentTypeDefinition typeDefinition);
        XElement Export(ContentPartDefinition partDefinition);
    }
}