using System;

namespace OrchardCore.Flows.Models
{
    public class BagPartSettings
    {
        public string[] ContainedContentTypes { get; set; } = Array.Empty<string>();
        public string DisplayType { get; set; }
    }
}