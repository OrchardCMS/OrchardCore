using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class RuleMigrator : IRuleMigrator
    {
        public void Migrate(string existingRule, Rule rule)
        {
            // Migrates well-known rules to well-known conditions.
            // A javascript condition is automatically created for less well-known rules. 
            switch (existingRule)
            {
                case "true":
                    rule.Conditions.Add(new BooleanCondition { Value = true });
                    break;
                case "isHomepage()":
                    rule.Conditions.Add(new HomepageCondition { Value = true });
                    break;
                case "isAnonymous()":
                    rule.Conditions.Add(new IsAnonymousCondition());
                    break;
                case "isAuthenticated()":
                    rule.Conditions.Add(new IsAuthenticatedCondition());
                    break;  
                default:
                    rule.Conditions.Add(new JavascriptCondition { Script = existingRule });
                    break;
            }
        }
    }
}