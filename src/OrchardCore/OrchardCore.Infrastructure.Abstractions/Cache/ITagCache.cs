using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Cache
{
    public interface ITagCache
    {
        void Tag(string key, params string[] tags);
        IEnumerable<string> GetTaggedItems(string tag);
        Task RemoveTagAsync(string tag);
    }

    public interface ITagRemovedEventHandler
    {
        Task TagRemovedAsync(string tag, IEnumerable<string> keys);
    }
}
