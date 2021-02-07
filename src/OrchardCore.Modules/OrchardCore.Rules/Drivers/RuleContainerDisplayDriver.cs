using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Rules.Drivers
{
    public class RuleContainerDisplayDriver : DisplayDriver<RuleContainer>
    {

        public override IDisplayResult Display(RuleContainer rule)
        {
            return Combine(
                View("RuleContainer_Fields_Summary", rule).Location("Summary", "Content"),
                Initialize<RuleGroupViewModel>("RuleGroup_Fields_Summary", m =>
                {
                    m.Entries = rule.Rules.Select(x => new RuleEntry { Rule = x}).ToArray();
                }).Location("Summary", "Content")
            );
        }
    }
}
