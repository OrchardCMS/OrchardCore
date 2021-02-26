using System.Collections.Generic;

namespace OrchardCore.Localization.Data.Models
{
    public class Translation
    {
        public string Context { get; set; }

        public string Key { get; set; }

        public IDictionary<string, string> Values { get; set; }
    }
}
