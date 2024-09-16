using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers;

public sealed class BooleanConditionDisplayDriver : DisplayDriver<Condition, BooleanCondition>
{
    public override Task<IDisplayResult> DisplayAsync(BooleanCondition condition, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("BooleanCondition_Fields_Summary", condition).Location("Summary", "Content"),
                View("BooleanCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(BooleanCondition condition, BuildEditorContext context)
    {
        return Initialize<BooleanConditionViewModel>("BooleanCondition_Fields_Edit", m =>
        {
            m.Value = condition.Value;
            m.Condition = condition;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(BooleanCondition condition, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(condition, Prefix, x => x.Value);

        return Edit(condition, context);
    }
}
