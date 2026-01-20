namespace OrchardCore.Data.Documents;

public interface IDocument
{
    /// <summary>
    /// The unique identifier of the document.
    /// </summary>
    string Identifier { get; set; }

    /// <summary>
    /// Whether the document was loaded for updating.
    /// </summary>
    bool IsReadOnly { get; set; }
}
