using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Orchard.Indexing
{
    public class IndexManager
    {
        public IDictionary<string, IIndexProvider> Providers { get; } = new ConcurrentDictionary<string, IIndexProvider>();
    }
}
