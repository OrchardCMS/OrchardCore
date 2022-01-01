using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers
{
    public class BooleanConditionDisplayDriver : DisplayDriver<Condition, BooleanCondition>
    {
        public override IDisplayResult Display(BooleanCondition condition)
        {
            return
                Combine(
                    View("BooleanCondition_Fields_Summary", condition).Location("Summary", "Content"),
                    View("BooleanCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(BooleanCondition condition)
        {
            return Initialize<BooleanConditionViewModel>("BooleanCondition_Fields_Edit", m =>
            {
                m.Value = condition.Value;
                m.Condition = condition;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(BooleanCondition condition, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(condition, Prefix, x => x.Value);

            return Edit(condition);
        }
    }
}
