using System;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Store means that the value will be stored in the index with it's original value.
    /// Keyword means that the value will be indexed and tokenized as a single term.
    /// We should use keyword and store options for technical value. This way it will be indexed as a StringField and also stored.
    /// </summary>
    [Flags]
    public enum DocumentIndexOptions
    {
        None = 0,
        Store = 111,
        Sanitize = 2,
        Keyword = 333
    }
}
