using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// An implementation of <see cref="IContentFieldIndexHandler"/> is able to take part in the rendering of
    /// a <see cref="ContentField"/> instance.
    /// </summary>
    public interface IContentFieldIndexHandler
    {
        Task BuildIndexAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition, BuildIndexContext context, IContentIndexSettings settings);
    }
}
