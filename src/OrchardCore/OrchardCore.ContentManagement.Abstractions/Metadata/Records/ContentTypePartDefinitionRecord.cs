using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    /// <summary>
    /// Represents a part and its settings in a type.
    /// </summary>
    public class ContentTypePartDefinitionRecord : IMessagePackSerializationCallbackReceiver
    {
        /// <summary>
        /// Gets or sets the part name.
        /// </summary>
        public string PartName { get; set; }

        /// <summary>
        /// Gets or sets the name of the part.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the settings of the part for this type.
        /// </summary>
        [IgnoreMember]
        public JObject Settings { get; set; }

        [JsonIgnore]
        public string JsonSettings { get; set; }

        public virtual void OnAfterDeserialize() => Settings = JObject.Parse(JsonSettings);

        public virtual void OnBeforeSerialize() => JsonSettings = Settings.ToString(Formatting.None);
    }
}
