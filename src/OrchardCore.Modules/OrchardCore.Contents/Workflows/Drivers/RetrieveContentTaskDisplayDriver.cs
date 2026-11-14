using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class RetrieveContentTaskDisplayDriver : ContentTaskDisplayDriver<RetrieveContentTask, RetrieveContentTaskViewModel>
{
    internal readonly IStringLocalizer S;

    public RetrieveContentTaskDisplayDriver(IStringLocalizer<RetrieveContentTaskDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(RetrieveContentTask activity, BuildEditorContext context)
    {
        return Initialize<RetrieveContentTaskViewModel>("RetrieveContentTask_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Content item"], "10"));
    }

    protected override void EditActivity(RetrieveContentTask activity, RetrieveContentTaskViewModel model)
    {
        model.ContentItemId = activity.Content.Expression;
    }

    protected override void UpdateActivity(RetrieveContentTaskViewModel model, RetrieveContentTask activity)
    {
        activity.Content = new WorkflowExpression<IContent>(model.ContentItemId);
    }
}
