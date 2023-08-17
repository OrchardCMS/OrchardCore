using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Drivers
{
    public class IsAnonymousConditionDisplayDriver : DisplayDriver<Condition, IsAnonymousCondition>
    {
        public override IDisplayResult Display(IsAnonymousCondition condition)
        {
            return
                Combine(
                    View("IsAnonymousCondition_Fields_Summary", condition).Location("Summary", "Content"),
                    View("IsAnonymousCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(IsAnonymousCondition condition)
        {
            return View("IsAnonymousCondition_Fields_Edit", condition).Location("Content");
        }
    }
}
