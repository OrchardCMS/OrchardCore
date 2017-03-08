using System.Threading.Tasks;
using Orchard.ContentManagement;

namespace Orchard.Indexing
{
    /// <summary>
    /// An implementation of <see cref="IContentItemIndexHandler"/> is able to take part in the rendering of
    /// a <see cref="ContentItem"/> instance.
    /// </summary>
    public interface IContentItemIndexHandler
    {
        Task BuildIndexAsync(BuildIndexContext context);
    }
}
