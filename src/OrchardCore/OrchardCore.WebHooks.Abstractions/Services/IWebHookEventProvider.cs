using System.Collections.Generic;
using OrchardCore.WebHooks.Abstractions.Models;

namespace OrchardCore.WebHooks.Abstractions.Services
{
    public interface IWebHookEventProvider
    {
        IEnumerable<WebHookEvent> GetEvents();

        IEnumerable<string> NormalizeEvents(IEnumerable<string> events);
    }
}