using Newtonsoft.Json.Linq;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A <see cref="Document"/> being an <see cref="IDocumentEntity"/>.
    /// </summary>
    public class DocumentEntity : Document, IDocumentEntity
    {
        public JObject Properties { get; set; } = new JObject();
    }
}
