using Newtonsoft.Json;

namespace OrchardCore.Data.Documents
{
    public class Document : IDocument
    {
        /// <summary>
        /// The <see cref="IDocument.Identifier"/>.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Whether the document was loaded for updating.
        /// </summary>
        [JsonIgnore]
        public bool isMutable { get; set; }
    }
}
