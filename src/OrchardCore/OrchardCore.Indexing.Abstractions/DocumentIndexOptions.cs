using System;

namespace OrchardCore.Indexing
{
    [Flags]
    public enum DocumentIndexOptions
    {
        None = 0,
        Store = 1,
        Sanitize = 4,
        Keyword = 5
    }
}
