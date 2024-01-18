using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Drivers
{
    public class IsAuthenticatedConditionDisplayDriver : DisplayDriver<Condition, IsAuthenticatedCondition>
    {
        public override IDisplayResult Display(IsAuthenticatedCondition condition)
        {
            return
                Combine(
                    View("IsAuthenticatedCondition_Fields_Summary", condition).Location("Summary", "Content"),
                    View("IsAuthenticatedCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(IsAuthenticatedCondition condition)
        {
            return View("IsAuthenticatedCondition_Fields_Edit", condition).Location("Content");
        }
    }
}
