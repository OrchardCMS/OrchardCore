using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers;

public sealed class CultureConditionDisplayDriver : DisplayDriver<Condition, CultureCondition>
{
    private readonly ConditionOperatorOptions _options;

    internal readonly IStringLocalizer S;

    public CultureConditionDisplayDriver(IOptions<ConditionOperatorOptions> options, IStringLocalizer<CultureConditionDisplayDriver> stringLocalizer)
    {
        _options = options.Value;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(CultureCondition condition, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("CultureCondition_Fields_Summary", condition).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("CultureCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(CultureCondition condition, BuildEditorContext context)
    {
        return Initialize<CultureConditionViewModel>("CultureCondition_Fields_Edit", m =>
            {
                if (condition.Operation != null && _options.ConditionOperatorOptionByType.TryGetValue(condition.Operation.GetType(), out var option))
                {
                    m.SelectedOperation = option.Factory.Name;
                }
                m.Value = condition.Value;
                m.Condition = condition;
            }).Location(l => l.Zone("Content").Card(S["Culture"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(CultureCondition condition, UpdateEditorContext context)
    {
        var model = new CultureConditionViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        condition.Value = model.Value;
        if (!string.IsNullOrEmpty(model.SelectedOperation) && _options.Factories.TryGetValue(model.SelectedOperation, out var factory))
        {
            condition.Operation = factory.Create();
        }

        return Edit(condition, context);
    }
}
