using System.Collections.Generic;

namespace OrchardCore.Environment.Extensions.Features
{
    public class FeatureOptions
    {
        public bool IncludeAll { get; set; }
        public HashSet<string> Include { get; set; } = new HashSet<string>();
        public HashSet<string> Exclude { get; set; } = new HashSet<string>();
    }
}
