using System.Text.Json.Serialization;

namespace OrchardCore.Data.Documents;

public class Document : IDocument
{
    /// <summary>
    /// The <see cref="IDocument.Identifier"/>.
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// Whether the document is immutable or not.
    /// </summary>
    [JsonIgnore]
    public bool IsReadOnly { get; set; } = true;
}
