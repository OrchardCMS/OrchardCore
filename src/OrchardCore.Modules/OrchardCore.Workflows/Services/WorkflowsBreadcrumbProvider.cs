using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Workflows.Models;
using YesSql;

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

    private readonly ISession _session;

    internal readonly IStringLocalizer S;

    public WorkflowsBreadcrumbProvider(ISession session, IStringLocalizer<WorkflowsBreadcrumbProvider> stringLocalizer)
    {
        _session = session;
        S = stringLocalizer;
    }

    public async ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case WorkflowsConstants.TypesList:
                AddWorkflows(builder);
                break;

            case WorkflowsConstants.TypeEdit:
                AddWorkflows(builder);
                builder.Add(builder.GetData<string>(WorkflowsConstants.TypeNameKey), item => item.Id("WorkflowType"));
                break;

            case WorkflowsConstants.TypeEditProperties:
                AddWorkflows(builder);
                builder.Add(S["Edit Workflow"], item => item.Id("WorkflowType"));
                break;

            case WorkflowsConstants.TypeDuplicate:
                AddWorkflows(builder);
                builder.Add(S["Copy Workflow"], item => item.Id("WorkflowType"));
                break;

            case WorkflowsConstants.Instances:
                AddWorkflows(builder);
                AddEditType(builder);
                builder.Add(S["Instances"], item => item.Id("Instances"));
                break;

            case WorkflowsConstants.InstanceDetails:
                AddWorkflows(builder);
                AddEditType(builder);
                AddInstances(builder);
                builder.Add(builder.GetData<string>(WorkflowsConstants.WorkflowIdKey), item => item.Id("Instance"));
                break;

            case WorkflowsConstants.ActivityCreate:
                AddWorkflows(builder);
                await AddTypeByIdAsync(builder);
                builder.Add(S["Add {0}", builder.GetData<string>(WorkflowsConstants.ActivityNameKey)],
                    item => item.Id("Activity"));
                break;

            case WorkflowsConstants.ActivityEdit:
                AddWorkflows(builder);
                await AddTypeByIdAsync(builder);
                builder.Add(S["Edit {0}", builder.GetData<string>(WorkflowsConstants.ActivityNameKey)],
                    item => item.Id("Activity"));
                break;
        }
    }

    // An activity is only reached from the workflow designer, so its trail leads back through the workflow.
    private async ValueTask AddTypeByIdAsync(BreadcrumbBuilder builder)
    {
        var typeId = builder.GetData<long>(WorkflowsConstants.TypeIdKey);

        if (typeId == 0)
        {
            return;
        }

        var workflowType = await _session.GetAsync<WorkflowType>(typeId);

        builder.Add(workflowType?.Name, item => item
            .Id("WorkflowType")
            .Action("Edit", "WorkflowType", new RouteValueDictionary(s_routeValues)
            {
                { "id", typeId },
            })
            .Permission(WorkflowsPermissions.ManageWorkflows));
    }

    private void AddWorkflows(BreadcrumbBuilder builder)
        => builder.Add(S["Workflows"], item => item
            .Id("WorkflowTypes")
            .Action("Index", "WorkflowType", s_routeValues)
            .Permission(WorkflowsPermissions.ManageWorkflows));

    private static void AddEditType(BreadcrumbBuilder builder)
        => builder.Add(builder.GetData<string>(WorkflowsConstants.TypeNameKey), item => item
            .Id("WorkflowType")
            .Action("Edit", "WorkflowType", new RouteValueDictionary(s_routeValues)
            {
                { "id", builder.GetData<string>(WorkflowsConstants.TypeIdKey) },
            })
            .Permission(WorkflowsPermissions.ManageWorkflows));

    private void AddInstances(BreadcrumbBuilder builder)
        => builder.Add(S["Instances"], item => item
            .Id("Instances")
            .Action("Index", "Workflow", new RouteValueDictionary(s_routeValues)
            {
                { "workflowTypeId", builder.GetData<string>(WorkflowsConstants.TypeIdKey) },
            })
            .Permission(WorkflowsPermissions.ManageWorkflows));
}
