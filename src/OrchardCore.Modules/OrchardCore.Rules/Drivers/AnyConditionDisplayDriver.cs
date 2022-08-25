using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers
{
    public class AnyConditionDisplayDriver : DisplayDriver<Condition, AnyConditionGroup>
    {
        public override IDisplayResult Display(AnyConditionGroup condition)
        {
            return
                Combine(
                    View("AnyCondition_Fields_Summary", condition).Location("Summary", "Content"),
                    View("AnyCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content"),
                    Initialize<ConditionGroupViewModel>("ConditionGroup_Fields_Summary", m =>
                    {
                        m.Entries = condition.Conditions.Select(x => new ConditionEntry { Condition = x }).ToArray();
                        m.Condition = condition;
                    }).Location("Summary", "Content")
                );
        }

        public override IDisplayResult Edit(AnyConditionGroup condition)
        {
            return Initialize<AnyConditionViewModel>("AnyCondition_Fields_Edit", m =>
            {
                m.DisplayText = condition.DisplayText;
                m.Condition = condition;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(AnyConditionGroup condition, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(condition, Prefix, x => x.DisplayText);

            return Edit(condition);
        }
    }
}
