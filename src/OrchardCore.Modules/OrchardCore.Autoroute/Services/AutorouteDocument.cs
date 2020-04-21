using System;
using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Data.Documents;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteDocument : Document, IMessagePackSerializationCallbackReceiver
    {
        public Dictionary<string, AutorouteEntry> Paths { get; set; } = new Dictionary<string, AutorouteEntry>();

        [IgnoreMember]
        public Dictionary<string, AutorouteEntry> ContentItemIds { get; set; } = new Dictionary<string, AutorouteEntry>(StringComparer.OrdinalIgnoreCase);

        [JsonIgnore]
        public Dictionary<string, AutorouteEntry> ContentItemIdsValues { get; set; }

        // 'MessagePack' doesn't preserve the 'OrdinalIgnoreCase' comparison when deserializing.
        public virtual void OnAfterDeserialize() => ContentItemIds = new Dictionary<string, AutorouteEntry>(ContentItemIdsValues, StringComparer.OrdinalIgnoreCase);
        public virtual void OnBeforeSerialize() => ContentItemIdsValues = ContentItemIds;
    }
}
