using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;

namespace Orchard.ContentManagement.MetaData
{
    public interface IContentDefinitionReader : IDependency
    {
        void Merge(XElement source, ContentTypeDefinitionBuilder builder);
        void Merge(XElement source, ContentPartDefinitionBuilder builder);
    }

    public static class ContentDefinitionReaderExtensions
    {
        public static ContentTypeDefinition Import(this IContentDefinitionReader reader, XElement source)
        {
            var target = new ContentTypeDefinitionBuilder();
            reader.Merge(source, target);
            return target.Build();
        }
    }
}