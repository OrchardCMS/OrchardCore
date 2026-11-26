using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class UnpublishContentTaskDisplayDriver : ContentTaskDisplayDriver<UnpublishContentTask, UnpublishContentTaskViewModel>
{
    internal readonly IStringLocalizer S;

    public UnpublishContentTaskDisplayDriver(IStringLocalizer<UnpublishContentTaskDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(UnpublishContentTask activity, BuildEditorContext context)
    {
        return Initialize<UnpublishContentTaskViewModel>("UnpublishContentTask_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Content item"], "10"));
    }

    protected override void EditActivity(UnpublishContentTask activity, UnpublishContentTaskViewModel model)
    {
        model.Expression = activity.Content.Expression;
    }

    protected override void UpdateActivity(UnpublishContentTaskViewModel model, UnpublishContentTask activity)
    {
        activity.Content = new WorkflowExpression<IContent>(model.Expression);
    }
}
