using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentTypeDefinitionRecord : IMessagePackSerializationCallbackReceiver
    {
        public ContentTypeDefinitionRecord()
        {
            ContentTypePartDefinitionRecords = new List<ContentTypePartDefinitionRecord>();
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }

        [IgnoreMember]
        public JObject Settings { get; set; }

        [JsonIgnore]
        public string JsonSettings { get; set; }

        public virtual void OnAfterDeserialize() => Settings = JObject.Parse(JsonSettings);

        public virtual void OnBeforeSerialize() => JsonSettings = Settings.ToString(Formatting.None);

        public IList<ContentTypePartDefinitionRecord> ContentTypePartDefinitionRecords { get; set; }
    }
}
