using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers;

public sealed class RuleDisplayDriver : DisplayDriver<Rule>
{
    public override Task<IDisplayResult> DisplayAsync(Rule rule, BuildDisplayContext context)
    {
        return CombineAsync(
            View("Rule_Fields_Summary", rule).Location("Summary", "Content"),
            Initialize<ConditionGroupViewModel>("ConditionGroup_Fields_Summary", m =>
            {
                m.Entries = rule.Conditions.Select(x => new ConditionEntry { Condition = x }).ToArray();
                m.Condition = rule;
            }).Location("Summary", "Content")
        );
    }
}
