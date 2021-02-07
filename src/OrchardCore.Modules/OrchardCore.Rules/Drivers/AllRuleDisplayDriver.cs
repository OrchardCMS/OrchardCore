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
    public class AllRuleDisplayDriver : DisplayDriver<Rule, AllRule>
    {
        public override IDisplayResult Display(AllRule rule)
        {
            return
                Combine(
                    View("AllRule_Fields_Summary", rule).Location("Summary", "Content"),
                    View("AllRule_Fields_Thumbnail", rule).Location("Thumbnail", "Content"),
                    Initialize<RuleGroupViewModel>("RuleGroup_Fields_Summary", m =>
                    {
                        m.Entries = rule.Rules.Select(x => new RuleEntry { Rule = x}).ToArray();
                    }).Location("Summary", "Content")
                );
        }

        public override IDisplayResult Edit(AllRule rule)
        {
            return Initialize<AllRuleViewModel>("AllRule_Fields_Edit", model =>
            {
                model.Name = rule.Name;
                model.Rule = rule;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(AllRule rule, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(rule, Prefix, x => x.Name);
          
            return Edit(rule);
        }
    }
}
