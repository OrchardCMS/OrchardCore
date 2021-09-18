using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Tenants.Models
{
    public class FeatureProfilesDocument : Document
    {
        public Dictionary<string, FeatureProfile> FeatureProfiles = new Dictionary<string, FeatureProfile>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Restricted Profile",
                new FeatureProfile
                {
                    FeatureRules = new List<FeatureRule>
                    {
                        new FeatureRule
                        {
                            Rule = "Exclude",
                            Expression = "OrchardCore.Templates"
                        },
                        new FeatureRule
                        {
                            Rule = "Exclude",
                            Expression = "TheAgencyTheme"
                        }
                    }
                }
            },
            {
                "All Profile",
                new FeatureProfile
                {
                    FeatureRules = new List<FeatureRule>
                    {
                        new FeatureRule
                        {
                            Rule = "Include",
                            Expression = "*"
                        }
                    }
                }
            }
        };
    }

}
