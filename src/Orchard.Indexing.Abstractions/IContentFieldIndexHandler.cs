using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.Indexing
{
    /// <summary>
    /// An implementation of <see cref="IContentFieldIndexHandler"/> is able to take part in the rendering of
    /// a <see cref="ContentField"/> instance.
    /// </summary>
    public interface IContentFieldIndexHandler
    {
        Task BuildIndexAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition, BuildIndexContext context, ContentIndexSettings settings);
    }
}
