using Orchard.Events;
using System.Collections.Generic;

namespace Orchard.Environment.Cache
{
    public interface ITagCache
    {
        void Tag(string key, params string[] tags);
        IEnumerable<string> GetTaggedItems(string tag);
        void RemoveTag(string tag);
    }

    public interface ITagRemovedEventHandler : IEventHandler
    {
        void TagRemoved(string tag, IEnumerable<string> keys);
    }
}
