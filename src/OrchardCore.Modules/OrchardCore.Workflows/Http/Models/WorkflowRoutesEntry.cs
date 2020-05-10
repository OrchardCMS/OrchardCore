using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Workflows.Http.Models
{
    public class WorkflowRoutesEntry : IMessagePackSerializationCallbackReceiver
    {
        public string WorkflowId { get; set; }
        public string ActivityId { get; set; }
        public string HttpMethod { get; set; }

        [IgnoreMember]
        public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();

        [JsonIgnore]
        public Dictionary<string, object> RouteDataValues { get; set; }

        // 'MessagePack' can't serialize a 'RouteValueDictionary'.
        public void OnAfterDeserialize() => RouteValues = new RouteValueDictionary(RouteDataValues);
        public void OnBeforeSerialize() => RouteDataValues = RouteValues.ToDictionary(kv => kv.Key, kv => kv.Value);

        public string ControllerName => RouteValues.GetValue<string>("controller");
        public string ActionName => RouteValues.GetValue<string>("action");
        public string AreaName => RouteValues.GetValue<string>("area");
    }
}
