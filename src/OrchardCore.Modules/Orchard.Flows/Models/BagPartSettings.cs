using System;

namespace Orchard.Flows.Models
{
    public class BagPartSettings
    {
        public string[] ContainedContentTypes { get; set; } = Array.Empty<string>();
    }
}