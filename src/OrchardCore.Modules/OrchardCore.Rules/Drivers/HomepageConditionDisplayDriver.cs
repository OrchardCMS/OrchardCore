using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers
{
    public class HomepageConditionDisplayDriver : DisplayDriver<Condition, HomepageCondition>
    {
        public override IDisplayResult Display(HomepageCondition condition)
        {
            return
                Combine(
                    View("HomepageCondition_Fields_Summary", condition).Location("Summary", "Content"),
                    View("HomepageCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(HomepageCondition condition)
        {
            return Initialize<HomepageConditionViewModel>("HomepageCondition_Fields_Edit", m =>
            {
                m.Value = condition.Value;
                m.Condition = condition;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(HomepageCondition condition, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(condition, Prefix, x => x.Value);

            return Edit(condition);
        }
    }
}
