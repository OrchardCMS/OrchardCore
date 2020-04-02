using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Entities
{
    public class Entity : IEntity, IMessagePackSerializationCallbackReceiver
    {
        [IgnoreMember]
        public JObject Properties { get; set; } = new JObject();

        [JsonIgnore]
        public string JsonProperties { get; set; }

        public void OnAfterDeserialize() => Properties = JObject.Parse(JsonProperties);

        public void OnBeforeSerialize() => JsonProperties = Properties.ToString(Formatting.None);
    }
}
