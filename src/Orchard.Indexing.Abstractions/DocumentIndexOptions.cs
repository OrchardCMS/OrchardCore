using System;

namespace Orchard.Indexing
{
    [Flags]
    public enum DocumentIndexOptions
    {
        None,
        Store,
        Analyze,
        Sanitize
    }
}
