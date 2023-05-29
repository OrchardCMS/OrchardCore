using System.Collections.Generic;

namespace OrchardCore.Data
{
    public class StoreCollectionOptions
    {
        public HashSet<string> Collections { get; } = new HashSet<string>();
    }
}
