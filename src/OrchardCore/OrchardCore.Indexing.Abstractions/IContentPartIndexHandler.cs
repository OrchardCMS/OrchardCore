using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// An implementation of <see cref="IContentPartIndexHandler"/> is able to take part in the rendering of
    /// a <see cref="ContentPart"/> instance.
    /// </summary>
    public interface IContentPartIndexHandler
    {
        Task BuildIndexAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildIndexContext context, IContentIndexSettings settings);
    }
}
