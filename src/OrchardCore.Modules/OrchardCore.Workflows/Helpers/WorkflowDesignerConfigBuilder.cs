using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Localization;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Helpers;

/// <summary>
/// Builds the JSON configuration of the designer app, rendered into the <c>data-config</c> attribute of
/// <c>#workflow-designer</c>. Every URL is generated here, so it includes the tenant prefix.
/// </summary>
internal static class WorkflowDesignerConfigBuilder
{
    private const string Area = "OrchardCore.Workflows";
    private const string DesignerController = "WorkflowDesigner";

    /// <summary>
    /// The configuration of the designer of <paramref name="workflowType"/>, or, when
    /// <paramref name="instance"/> is set, of the read-only viewer of that instance.
    /// <paramref name="initialActivityId"/> is the activity the designer selects and opens when it loads.
    /// </summary>
    public static string Build(
        IUrlHelper url,
        ClaimsPrincipal user,
        IEnumerable<IJSLocalizer> localizers,
        WorkflowType workflowType,
        Workflow instance = null,
        string initialActivityId = null)
    {
        var route = new { area = Area, workflowTypeId = workflowType.Id };

        var urls = instance is null
            ? new
            {
                Definition = url.Action("Definition", DesignerController, route),
                Library = url.Action("Library", DesignerController, route),
                Save = url.Action("Save", DesignerController, route),
                AddActivity = url.Action("AddActivity", DesignerController, route),
                Editor = url.Action("Editor", DesignerController, route),
                Settings = url.Action("Settings", DesignerController, route),
                Publish = url.Action("Publish", DesignerController, route),
                Discard = url.Action("Discard", DesignerController, route),
                Versions = url.Action("Versions", DesignerController, route),
                Version = url.Action("Version", DesignerController, route),
                Compare = url.Action("Compare", DesignerController, route),
                Restore = url.Action("Restore", DesignerController, route),
            }
            : new
            {
                // The viewer only loads the instance; it never changes the workflow type.
                Definition = url.Action("Instance", DesignerController, new { area = Area, workflowTypeId = workflowType.Id, instanceId = instance.Id }),
                Library = (string)null,
                Save = (string)null,
                AddActivity = (string)null,
                Editor = (string)null,
                Settings = (string)null,
                Publish = (string)null,
                Discard = (string)null,
                Versions = (string)null,
                Version = (string)null,
                Compare = (string)null,
                Restore = (string)null,
            };

        var config = new
        {
            WorkflowTypeId = workflowType.Id,
            ReadOnly = instance is not null,
            Urls = urls,
            InstancesUrl = url.Action("Index", "Workflow", route),
            ExportUrl = url.Action("Export", "WorkflowType", new { area = Area, id = workflowType.Id }),
            ListUrl = url.Action("Index", "WorkflowType", new { area = Area }),
            CurrentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
            InitialActivityId = string.IsNullOrEmpty(initialActivityId) ? null : initialActivityId,
            Translations = localizers.GetMergedLocalizations(WorkflowsDesignerJSLocalizer.Group),
        };

        return JsonSerializer.Serialize(config, JOptions.CamelCase);
    }
}
