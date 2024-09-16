using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class CreateContentTaskDisplayDriver : ContentTaskDisplayDriver<CreateContentTask, CreateContentTaskViewModel>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public CreateContentTaskDisplayDriver(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    protected override async ValueTask EditActivityAsync(CreateContentTask activity, CreateContentTaskViewModel model)
    {
        model.AvailableContentTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Select(x => new SelectListItem { Text = x.DisplayName, Value = x.Name })
            .ToList();

        model.ContentType = activity.ContentType;
        model.Publish = activity.Publish;
        model.ContentProperties = activity.ContentProperties.Expression;
    }

    protected override void UpdateActivity(CreateContentTaskViewModel model, CreateContentTask activity)
    {
        activity.ContentType = model.ContentType;
        activity.Publish = model.Publish;
        activity.ContentProperties = new WorkflowExpression<string>(model.ContentProperties);
    }
}
