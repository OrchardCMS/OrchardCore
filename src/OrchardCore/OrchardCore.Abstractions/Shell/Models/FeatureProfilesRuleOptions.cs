using System;
using System.Collections.Generic;

namespace OrchardCore.Environment.Shell.Models
{
    /// <summary>
    /// Provides options for validating features through profiles.
    /// </summary>
    public class FeatureProfilesRuleOptions
    {
        public Dictionary<string, Func<string, string, (bool isMatch, bool isAllowed)>> Rules = new(StringComparer.OrdinalIgnoreCase);
    }
}
