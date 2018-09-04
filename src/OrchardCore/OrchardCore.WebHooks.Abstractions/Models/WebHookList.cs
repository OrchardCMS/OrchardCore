using System.Collections.Generic;

namespace OrchardCore.WebHooks.Abstractions.Models
{
    public class WebHookList
    {
        public List<WebHook> WebHooks { get; set; } = new List<WebHook>();
    }
}