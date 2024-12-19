using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Drivers;

public sealed class IsAuthenticatedConditionDisplayDriver : DisplayDriver<Condition, IsAuthenticatedCondition>
{
    public override Task<IDisplayResult> DisplayAsync(IsAuthenticatedCondition condition, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("IsAuthenticatedCondition_Fields_Summary", condition).Location("Summary", "Content"),
                View("IsAuthenticatedCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(IsAuthenticatedCondition condition, BuildEditorContext context)
    {
        return View("IsAuthenticatedCondition_Fields_Edit", condition).Location("Content");
    }
}
