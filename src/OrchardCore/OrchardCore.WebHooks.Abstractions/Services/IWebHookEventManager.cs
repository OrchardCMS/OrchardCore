using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.WebHooks.Abstractions.Models;

namespace OrchardCore.WebHooks.Abstractions.Services
{
    public interface IWebHookEventManager
    {
        Task<List<WebHookEvent>> GetAllWebHookEventsAsync();

        Task<HashSet<string>> NormalizeEventsAsync(IEnumerable<string> events);
    }
}
