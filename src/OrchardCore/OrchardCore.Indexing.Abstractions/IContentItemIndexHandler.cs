using System.Threading.Tasks;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// An implementation of <see cref="IContentItemIndexHandler"/> can provide property values for an index document.
    /// </summary>
    public interface IContentItemIndexHandler
    {
        Task BuildIndexAsync(BuildIndexContext context);
    }
}
