using System;
using System.Collections.Specialized;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.ViewModels
{
    public class FlowPartSettingsViewModel
    {
        public FlowPartSettings FlowPartSettings { get; set; }
        public NameValueCollection ContentTypes { get; set; }
        public string[] ContainedContentTypes { get; set; } = Array.Empty<string>();
    }
}
