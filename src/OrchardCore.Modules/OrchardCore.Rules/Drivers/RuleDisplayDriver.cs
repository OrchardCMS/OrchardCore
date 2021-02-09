using System;
using System.Linq;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Rules.Drivers
{
    public class RuleDisplayDriver : DisplayDriver<Rule>
    {

        public override IDisplayResult Display(Rule rule)
        {
            return Combine(
                View("Rule_Fields_Summary", rule).Location("Summary", "Content"),
                Initialize<ConditionGroupViewModel>("ConditionGroup_Fields_Summary", m =>
                {
                    m.Entries = rule.Conditions.Select(x => new ConditionEntry { Condition = x}).ToArray();
                }).Location("Summary", "Content")
            );
        }
    }
}
