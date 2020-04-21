using System;
using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using OrchardCore.Data.Documents;

namespace OrchardCore.Queries.Services
{
    public class QueriesDocument : Document, IMessagePackSerializationCallbackReceiver
    {
        [IgnoreMember]
        public Dictionary<string, Query> Queries { get; set; } = new Dictionary<string, Query>(StringComparer.OrdinalIgnoreCase);

        [JsonIgnore]
        public Dictionary<string, Query> QueriesValues { get; set; }

        // 'MessagePack' doesn't preserve the 'OrdinalIgnoreCase' comparison when deserializing.
        public virtual void OnAfterDeserialize() => Queries = new Dictionary<string, Query>(QueriesValues, StringComparer.OrdinalIgnoreCase);
        public virtual void OnBeforeSerialize() => QueriesValues = Queries;
    }
}
