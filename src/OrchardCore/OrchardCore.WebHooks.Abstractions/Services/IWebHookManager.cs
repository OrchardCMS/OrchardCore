using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.WebHooks.Abstractions.Services
{
    public interface IWebHookManager
    {
        Task NotifyAsync(string eventName, JObject defaultPayload, Dictionary<string, object> properties);
    }
}