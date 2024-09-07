using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class UpdateContentTaskDisplayDriver : ContentTaskDisplayDriver<UpdateContentTask, UpdateContentTaskViewModel>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public UpdateContentTaskDisplayDriver(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
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
