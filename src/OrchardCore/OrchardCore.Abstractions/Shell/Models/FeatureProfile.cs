using System.Collections.Generic;

namespace OrchardCore.Environment.Shell.Models
{
    public class FeatureProfile
    {
        public List<FeatureRule> FeatureRules = new List<FeatureRule>();
    }

    public class FeatureRule
    {
        public string Rule { get; set; }
        public string Expression { get; set; }
    }
}
