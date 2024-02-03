using System.Text.Json.Nodes;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents;

/// <summary>
/// A <see cref="Document"/> being an <see cref="IDocumentEntity"/>.
/// </summary>
public class DocumentEntity : Document, IDocumentEntity
{
    public JsonObject Properties { get; set; } = [];
}
