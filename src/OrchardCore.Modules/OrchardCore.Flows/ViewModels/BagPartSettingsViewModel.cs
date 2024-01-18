using System;
using System.Collections.Specialized;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.ViewModels
{
    public class BagPartSettingsViewModel
    {
        public BagPartSettings BagPartSettings { get; set; }
        public NameValueCollection ContentTypes { get; set; }
        public string DisplayType { get; set; }
        public string[] ContainedContentTypes { get; set; } = Array.Empty<string>();
        public BagPartSettingType Source { get; set; }
        public string Stereotypes { get; set; }
    }
}
