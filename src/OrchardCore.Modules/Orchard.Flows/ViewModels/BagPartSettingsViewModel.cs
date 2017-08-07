using System;
using System.Collections.Specialized;
using Orchard.Flows.Models;

namespace Orchard.Flows.ViewModels
{
    public class BagPartSettingsViewModel
    {
        public BagPartSettings BagPartSettings { get; set; }
        public NameValueCollection ContentTypes { get; set; }
        public string[] ContainedContentTypes { get; set; } = Array.Empty<string>();
    }
}
