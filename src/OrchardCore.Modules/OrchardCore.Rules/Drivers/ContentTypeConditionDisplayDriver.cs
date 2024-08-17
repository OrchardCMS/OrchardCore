using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers;

public sealed class ContentTypeConditionDisplayDriver : DisplayDriver<Condition, ContentTypeCondition>
{
    private readonly ConditionOperatorOptions _options;

    public ContentTypeConditionDisplayDriver(IOptions<ConditionOperatorOptions> options)
    {
        _options = options.Value;
    }

    public override Task<IDisplayResult> DisplayAsync(ContentTypeCondition condition, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("ContentTypeCondition_Fields_Summary", condition).Location("Summary", "Content"),
                View("ContentTypeCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ContentTypeCondition condition, BuildEditorContext context)
    {
        return Initialize<ContentTypeConditionViewModel>("ContentTypeCondition_Fields_Edit", m =>
        {
            if (condition.Operation != null && _options.ConditionOperatorOptionByType.TryGetValue(condition.Operation.GetType(), out var option))
            {
                m.SelectedOperation = option.Factory.Name;
            }
            m.Value = condition.Value;
            m.Condition = condition;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeCondition condition, UpdateEditorContext context)
    {
        var model = new ContentTypeConditionViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        condition.Value = model.Value;
        if (!string.IsNullOrEmpty(model.SelectedOperation) && _options.Factories.TryGetValue(model.SelectedOperation, out var factory))
        {
            condition.Operation = factory.Create();
        }

        return Edit(condition, context);
    }
}
