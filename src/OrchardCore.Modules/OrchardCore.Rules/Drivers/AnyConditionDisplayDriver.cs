using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers;

public sealed class AnyConditionDisplayDriver : DisplayDriver<Condition, AnyConditionGroup>
{
    internal readonly IStringLocalizer S;

    public AnyConditionDisplayDriver(IStringLocalizer<AnyConditionDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AnyConditionGroup condition, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AnyCondition_Fields_Summary", condition).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AnyCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content"),
                Initialize<ConditionGroupViewModel>("ConditionGroup_Fields_Summary", m =>
                {
                    m.Entries = condition.Conditions.Select(x => new ConditionEntry { Condition = x }).ToArray();
                    m.Condition = condition;
                }).Location(OrchardCoreConstants.DisplayType.Summary, "Content")
            );
    }

    public override IDisplayResult Edit(AnyConditionGroup condition, BuildEditorContext context)
    {
        return Initialize<AnyConditionViewModel>("AnyCondition_Fields_Edit", m =>
        {
            m.DisplayText = condition.DisplayText;
            m.Condition = condition;
        }).Location(l => l.Zone("Content").Card(S["Any"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(AnyConditionGroup condition, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(condition, Prefix, x => x.DisplayText);

        return Edit(condition, context);
    }
}
