using System;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Store means that the value will be stored in the index with it's original value.
    /// Keyword means that the value will be indexed and tokenized as a single term. See Lucene StringField.
    /// We should use keyword and store options for technical value. This way it will be using a StringField but stored.
    /// Stored and Keyword are not the same. Stored is an override to say that we want to store the original value in the index.
    /// See Elasticsearch documentation : <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping-store.html#mapping-store"/>
    /// </summary>
    [Flags]
    public enum DocumentIndexOptions
    {
        None = 1,
        Store = 2,
        Sanitize = 4,
        Keyword = 8
    }
}
