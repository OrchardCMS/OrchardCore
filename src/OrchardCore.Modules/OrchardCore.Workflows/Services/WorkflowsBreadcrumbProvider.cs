using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Describes the breadcrumb trails of the workflows screens.
/// </summary>
public sealed class WorkflowsBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Workflows" },
    };

    internal readonly IStringLocalizer S;

    public WorkflowsBreadcrumbProvider(IStringLocalizer<WorkflowsBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case WorkflowsBreadcrumbs.TypesList:
                AddWorkflows(builder);
                break;

            case WorkflowsBreadcrumbs.TypeEdit:
                AddWorkflows(builder);
                builder.Add(builder.GetData<string>(WorkflowsBreadcrumbs.TypeNameKey), item => item.Id("WorkflowType"));
                break;

            case WorkflowsBreadcrumbs.TypeEditProperties:
                AddWorkflows(builder);
                builder.Add(S["Edit Workflow"], item => item.Id("WorkflowType"));
                break;

            case WorkflowsBreadcrumbs.TypeDuplicate:
                AddWorkflows(builder);
                builder.Add(S["Copy Workflow"], item => item.Id("WorkflowType"));
                break;

            case WorkflowsBreadcrumbs.Instances:
                AddWorkflows(builder);
                AddEditType(builder);
                builder.Add(S["Instances"], item => item.Id("Instances"));
                break;

            case WorkflowsBreadcrumbs.InstanceDetails:
                AddWorkflows(builder);
                AddEditType(builder);
                AddInstances(builder);
                builder.Add(builder.GetData<string>(WorkflowsBreadcrumbs.WorkflowIdKey), item => item.Id("Instance"));
                break;

            case WorkflowsBreadcrumbs.ActivityCreate:
                AddWorkflows(builder);
                builder.Add(S["Add {0}", builder.GetData<string>(WorkflowsBreadcrumbs.ActivityNameKey)],
                    item => item.Id("Activity"));
                break;

            case WorkflowsBreadcrumbs.ActivityEdit:
                AddWorkflows(builder);
                builder.Add(S["Edit {0}", builder.GetData<string>(WorkflowsBreadcrumbs.ActivityNameKey)],
                    item => item.Id("Activity"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddWorkflows(BreadcrumbBuilder builder)
        => builder.Add(S["Workflows"], item => item
            .Id("WorkflowTypes")
            .Action("Index", "WorkflowType", s_routeValues)
            .Permission(WorkflowsPermissions.ManageWorkflows));

    private void AddEditType(BreadcrumbBuilder builder)
        => builder.Add(builder.GetData<string>(WorkflowsBreadcrumbs.TypeNameKey), item => item
            .Id("WorkflowType")
            .Action("Edit", "WorkflowType", new RouteValueDictionary(s_routeValues)
            {
                { "id", builder.GetData<string>(WorkflowsBreadcrumbs.TypeIdKey) },
            })
            .Permission(WorkflowsPermissions.ManageWorkflows));

    private void AddInstances(BreadcrumbBuilder builder)
        => builder.Add(S["Instances"], item => item
            .Id("Instances")
            .Action("Index", "Workflow", new RouteValueDictionary(s_routeValues)
            {
                { "workflowTypeId", builder.GetData<string>(WorkflowsBreadcrumbs.TypeIdKey) },
            })
            .Permission(WorkflowsPermissions.ManageWorkflows));
}
