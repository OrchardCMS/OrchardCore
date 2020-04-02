using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Data.Documents;
using OrchardCore.Entities;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A <see cref="BaseDocument"/> being an <see cref="IEntity"/> that is serializable by 'MessagePack'.
    /// </summary>
    public abstract class DocumentEntity : BaseDocument, IEntity, IMessagePackSerializationCallbackReceiver
    {
        [IgnoreMember]
        public JObject Properties { get; set; } = new JObject();

        [JsonIgnore]
        public string JsonProperties { get; set; }

        public virtual void OnAfterDeserialize() => Properties = JObject.Parse(JsonProperties);

        public virtual void OnBeforeSerialize() => JsonProperties = Properties.ToString(Formatting.None);
    }
}
