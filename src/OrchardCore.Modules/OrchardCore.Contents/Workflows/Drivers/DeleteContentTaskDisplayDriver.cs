using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class DeleteContentTaskDisplayDriver : ContentTaskDisplayDriver<DeleteContentTask, DeleteContentTaskViewModel>
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public DeleteContentTaskDisplayDriver(IStringLocalizer<DeleteContentTaskDisplayDriver> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(DeleteContentTask activity, BuildEditorContext context)
    {
        return Initialize<DeleteContentTaskViewModel>("DeleteContentTask_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(_localizerFactory.Localize(CommonLocalizationSources.ContentItem), "10"));
    }

    protected override void EditActivity(DeleteContentTask activity, DeleteContentTaskViewModel model)
    {
        model.Expression = activity.Content.Expression;
    }

    protected override void UpdateActivity(DeleteContentTaskViewModel model, DeleteContentTask activity)
    {
        activity.Content = new WorkflowExpression<IContent>(model.Expression);
    }
}
