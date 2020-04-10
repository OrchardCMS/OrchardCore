using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentPartDefinitionRecord : IMessagePackSerializationCallbackReceiver
    {
        public ContentPartDefinitionRecord()
        {
            ContentPartFieldDefinitionRecords = new List<ContentPartFieldDefinitionRecord>();
            Settings = new JObject();
        }

        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the settings of a part, like description, or any property that a module would attach
        /// to a part.
        /// </summary>
        [IgnoreMember]
        public JObject Settings { get; set; }

        [JsonIgnore]
        public string JsonSettings { get; set; }

        public virtual void OnAfterDeserialize() => Settings = JObject.Parse(JsonSettings);

        public virtual void OnBeforeSerialize() => JsonSettings = Settings.ToString(Formatting.None);

        public IList<ContentPartFieldDefinitionRecord> ContentPartFieldDefinitionRecords { get; set; }
    }
}
