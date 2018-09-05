using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OrchardCore.WebHooks.Abstractions.Models
{
    public class WebHookNotificationContext
    {
        public string EventName { get; set; }

        public JObject DefaultPayload { get; set; }

        public IDictionary<string, object> Properties { get; set; }
    }
}