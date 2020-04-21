using System;
using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using OrchardCore.Data.Documents;

namespace OrchardCore.Templates.Models
{
    public class TemplatesDocument : Document, IMessagePackSerializationCallbackReceiver
    {
        [IgnoreMember]
        public Dictionary<string, Template> Templates { get; set; } = new Dictionary<string, Template>(StringComparer.OrdinalIgnoreCase);

        [JsonIgnore]
        public Dictionary<string, Template> TemplatesValues { get; set; }

        // 'MessagePack' doesn't preserve the 'OrdinalIgnoreCase' comparison when deserializing.
        public virtual void OnAfterDeserialize() => Templates = new Dictionary<string, Template>(TemplatesValues, StringComparer.OrdinalIgnoreCase);
        public virtual void OnBeforeSerialize() => TemplatesValues = Templates;
    }

    public class Template
    {
        public string Content { get; set; }
        public string Description { get; set; }
    }
}
