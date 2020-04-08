using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A <see cref="BaseDocument"/> being an <see cref="IDocumentEntity"/> that is serializable by 'MessagePack'.
    /// </summary>
    public class DocumentEntity : BaseDocument, IDocumentEntity
    {
        [IgnoreMember]
        public JObject Properties { get; set; } = new JObject();

        [JsonIgnore]
        public string JsonProperties { get; set; }

        public virtual void OnAfterDeserialize() => Properties = JObject.Parse(JsonProperties);

        public virtual void OnBeforeSerialize() => JsonProperties = Properties.ToString(Formatting.None);
    }
}
