namespace OrchardCore.Indexing;

public class BuildDocumentIndexContext
{
    public BuildDocumentIndexContext(
        DocumentIndex documentIndex,
        object record,
        IList<string> keys,
        IContentIndexSettings settings
    )
    {
        Record = record;
        DocumentIndex = documentIndex;
        Keys = keys;
        Settings = settings;
    }

    public IList<string> Keys { get; }

    public object Record { get; }

    public DocumentIndex DocumentIndex { get; }

    public IContentIndexSettings Settings { get; }

    /// <summary>
    /// Provides a dictionary to store additional data that can be used by index providers.
    /// </summary>
    public IDictionary<string, object> AdditionalProperties { get; set; }
}
