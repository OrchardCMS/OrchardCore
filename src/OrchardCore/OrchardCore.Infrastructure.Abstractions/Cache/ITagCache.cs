using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Cache
{
    public interface ITagCache
    {
        Task TagAsync(string key, params string[] tags);
        Task<IEnumerable<string>> GetTaggedItemsAsync(string tag);
        Task RemoveTagAsync(string tag);
    }

    public interface ITagRemovedEventHandler
    {
        Task TagRemovedAsync(string tag, IEnumerable<string> keys);
    }
}
