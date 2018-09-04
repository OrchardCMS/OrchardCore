using System.Collections.Generic;
using OrchardCore.WebHooks.Abstractions.Models;

namespace OrchardCore.WebHooks.ViewModels
{
    public class EditWebHookViewModel
    {
        public List<WebHookEvent> Events { get; set; }

        public WebHook WebHook { get; set; } = new WebHook();

        public bool CustomPayload { get; set; }

        public bool SubscribeAllEvents { get; set; } = true;
    }
}