using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.WebHooks.Abstractions.Models;

namespace OrchardCore.WebHooks.Expressions
{
    public interface IWebHookExpressionEvaluator
    {
        Task<JObject> RenderAsync(WebHook webHook, WebHookNotificationContext context);
    }
}