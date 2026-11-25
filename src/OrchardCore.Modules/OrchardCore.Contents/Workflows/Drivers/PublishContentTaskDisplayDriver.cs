using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class PublishContentTaskDisplayDriver : ContentTaskDisplayDriver<PublishContentTask, PublishContentTaskViewModel>
{
    internal readonly IStringLocalizer S;

    public PublishContentTaskDisplayDriver(IStringLocalizer<PublishContentTaskDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(PublishContentTask activity, BuildEditorContext context)
    {
        return Initialize<PublishContentTaskViewModel>("PublishContentTask_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Content item"], "10"));
    }

    protected override void EditActivity(PublishContentTask activity, PublishContentTaskViewModel model)
    {
        model.Expression = activity.Content.Expression;
    }

    protected override void UpdateActivity(PublishContentTaskViewModel model, PublishContentTask activity)
    {
        activity.Content = new WorkflowExpression<IContent>(model.Expression);
    }
}
