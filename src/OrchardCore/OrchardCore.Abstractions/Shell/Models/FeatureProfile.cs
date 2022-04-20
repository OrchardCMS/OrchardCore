using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Environment.Shell.Models
{
    public class FeatureProfile
    {
        public List<FeatureRule> FeatureRules = new List<FeatureRule>();
    }

    public class FeatureRule
    {
        [Required]
        public string Rule { get; set; }

        [Required]
        public string Expression { get; set; }
    }
}
