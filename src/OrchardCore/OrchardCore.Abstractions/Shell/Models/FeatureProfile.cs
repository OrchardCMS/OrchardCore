using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell.Models
{
    public class FeatureProfile
    {
        public JObject Properties { get; set; } = new JObject();

        public string Name { get; set; }

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
