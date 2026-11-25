using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Drivers;

public sealed class IsAuthenticatedConditionDisplayDriver : DisplayDriver<Condition, IsAuthenticatedCondition>
{
    internal readonly IStringLocalizer S;

    public IsAuthenticatedConditionDisplayDriver(IStringLocalizer<IsAuthenticatedConditionDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(IsAuthenticatedCondition condition, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("IsAuthenticatedCondition_Fields_Summary", condition).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("IsAuthenticatedCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(IsAuthenticatedCondition condition, BuildEditorContext context)
    {
        return View("IsAuthenticatedCondition_Fields_Edit", condition).Location(l => l.Zone("Content").Card(S["Is authenticated"], "10"));
    }
}
