using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class CreateContentTaskDisplayDriver : ContentTaskDisplayDriver<CreateContentTask, CreateContentTaskViewModel>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    internal readonly IStringLocalizer S;

    public CreateContentTaskDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<CreateContentTaskDisplayDriver> stringLocalizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CreateContentTask activity, BuildEditorContext context)
    {
        return Initialize<CreateContentTaskViewModel>("CreateContentTask_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Content item"], "10"));
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
