using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class UpdateContentTaskDisplayDriver : ContentTaskDisplayDriver<UpdateContentTask, UpdateContentTaskViewModel>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    internal readonly IStringLocalizer S;

    public UpdateContentTaskDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<UpdateContentTaskDisplayDriver> stringLocalizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(UpdateContentTask activity, BuildEditorContext context)
    {
        return Initialize<UpdateContentTaskViewModel>("UpdateContentTask_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Content item"], "10"));
    }

    protected override async ValueTask EditActivityAsync(UpdateContentTask activity, UpdateContentTaskViewModel model)
    {
        model.AvailableContentTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Select(x => new SelectListItem { Text = x.DisplayName, Value = x.Name })
            .ToList();

        model.ContentItemIdExpression = activity.Content.Expression;
        model.ContentProperties = activity.ContentProperties.Expression;
        model.Publish = activity.Publish;
    }

    protected override void UpdateActivity(UpdateContentTaskViewModel model, UpdateContentTask activity)
    {
        activity.Content = new WorkflowExpression<IContent>(model.ContentItemIdExpression);
        activity.ContentProperties = new WorkflowExpression<string>(model.ContentProperties);
        activity.Publish = model.Publish;
    }
}
