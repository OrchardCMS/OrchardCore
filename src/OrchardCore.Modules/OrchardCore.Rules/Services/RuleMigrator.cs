using System;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class RuleMigrator : IRuleMigrator
    {
        public void Migrate(string existingRule, Rule rule)
        {
            if (String.Equals(existingRule, "true", StringComparison.OrdinalIgnoreCase))
            {
                rule.Conditions.Add(new BooleanCondition { Value = true });
            }
            else if (String.Equals(existingRule, "isHomepage()", StringComparison.OrdinalIgnoreCase))
            {
                rule.Conditions.Add(new HomepageCondition());
            }
            else
            {
                rule.Conditions.Add(new JavascriptCondition { Script = existingRule });
            }
        }
    }
}