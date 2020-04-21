using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A <see cref="Document"/> being an <see cref="IDocumentEntity"/> that is serializable by 'MessagePack'.
    /// </summary>
    public class DocumentEntity : Document, IDocumentEntity
    {
        [IgnoreMember]
        public JObject Properties { get; set; } = new JObject();

        [JsonIgnore]
        public string JsonProperties { get; set; }

        // 'MessagePack' can't serialize a 'JObject'.
        public virtual void OnAfterDeserialize() => Properties = JObject.Parse(JsonProperties);
        public virtual void OnBeforeSerialize() => JsonProperties = Properties.ToString(Formatting.None);
    }
}
