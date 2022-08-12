using System;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Store means that the value will be stored in the index.
    /// Keyword means that the value will be indexed and tokenized as a single term. See Lucene StringField.
    /// We should use keyword and store options together for technical values. This way it will be using a StringField but also be stored.
    /// Stored and Keyword are not the same.
    /// If you absolutely need to only store the original value of a field then you should use a Lucene StoredField.
    /// See Elasticsearch documentation : <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping-store.html#mapping-store"/>
    /// </summary>
    [Flags]
    public enum DocumentIndexOptions
    {
        None = 0,
        Store = 1,
        Sanitize = 2,
        Keyword = 4
    }
}
