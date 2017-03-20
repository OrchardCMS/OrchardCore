using System.Collections.Generic;

namespace OrchardCore.Extensions
{
    public class ExtensionExpanderOptions
    {
        public IList<ExtensionExpanderOption> Options { get; }
            = new List<ExtensionExpanderOption>();
    }

    public class ExtensionExpanderOption
    {
        public string SearchPath { get; set; }
    }
}