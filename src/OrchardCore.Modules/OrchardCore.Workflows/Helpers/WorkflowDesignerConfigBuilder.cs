using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Localization;
using OrchardCore.Workflows.RealTime;
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
    /// <paramref name="initialActivityId"/> is the activity the designer selects and opens when it loads, and
    /// <paramref name="canRetry"/> whether the viewer offers to retry a faulted instance.
    /// </summary>
    public static string Build(
        IUrlHelper url,
        ClaimsPrincipal user,
        IEnumerable<IJSLocalizer> localizers,
        WorkflowType workflowType,
        Workflow instance = null,
        string initialActivityId = null,
        bool canRetry = false)
    {
        var route = new { area = Area, workflowTypeId = workflowType.Id };

        var urls = instance is null
            ? new DesignerUrls
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
                Restore = url.Action("Restore", DesignerController, route),
                Variables = url.Action("Variables", DesignerController, route),
                OutputBindings = url.Action("OutputBindings", DesignerController, route),
            }
            : new DesignerUrls
            {
                // The viewer only loads the instance, the data of its journal records, and retries it; it never changes
                // the workflow type.
                Definition = url.Action("Instance", DesignerController, new { area = Area, workflowTypeId = workflowType.Id, instanceId = instance.Id }),
                JournalData = url.Action("JournalData", DesignerController, new { area = Area, workflowTypeId = workflowType.Id, instanceId = instance.Id }),
                Retry = canRetry ? url.Action("Retry", DesignerController, route) : null,
            };

        return Serialize(url, user, localizers, workflowType, instance is null ? "designer" : "instance", urls, initialActivityId);
    }

    /// <summary>
    /// The configuration of the read-only page of a version of <paramref name="workflowType"/>.
    /// </summary>
    public static string BuildForVersion(IUrlHelper url, ClaimsPrincipal user, IEnumerable<IJSLocalizer> localizers, WorkflowType workflowType, string versionId)
    {
        var urls = new DesignerUrls
        {
            Definition = url.Action("Version", DesignerController, new { area = Area, workflowTypeId = workflowType.Id, versionId }),
        };

        return Serialize(url, user, localizers, workflowType, "version", urls);
    }

    /// <summary>
    /// The configuration of the page that compares two definitions of <paramref name="workflowType"/>: version
    /// ids, or <c>draft</c>.
    /// </summary>
    public static string BuildForComparison(IUrlHelper url, ClaimsPrincipal user, IEnumerable<IJSLocalizer> localizers, WorkflowType workflowType, string from, string to)
    {
        var urls = new DesignerUrls
        {
            Compare = url.Action("Compare", DesignerController, new { area = Area, workflowTypeId = workflowType.Id, from, to }),
        };

        return Serialize(url, user, localizers, workflowType, "compare", urls);
    }

    private static string Serialize(
        IUrlHelper url,
        ClaimsPrincipal user,
        IEnumerable<IJSLocalizer> localizers,
        WorkflowType workflowType,
        string mode,
        DesignerUrls urls,
        string initialActivityId = null)
    {
        var route = new { area = Area, workflowTypeId = workflowType.Id };

        var config = new
        {
            WorkflowTypeId = workflowType.Id,
            Mode = mode,
            ReadOnly = mode != "designer",
            Urls = urls,
            DesignerUrl = url.Action("Edit", "WorkflowType", new { area = Area, id = workflowType.Id }),
            VersionPageUrl = url.Action("Version", "WorkflowType", new { area = Area, id = workflowType.Id }),
            ComparePageUrl = url.Action("CompareVersions", "WorkflowType", new { area = Area, id = workflowType.Id }),
            InstancesUrl = url.Action("Index", "Workflow", route),
            ExportUrl = url.Action("Export", "WorkflowType", new { area = Area, id = workflowType.Id }),
            ListUrl = url.Action("Index", "WorkflowType", new { area = Area }),
            CurrentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
            InitialActivityId = string.IsNullOrEmpty(initialActivityId) ? null : initialActivityId,
            HubUrl = WorkflowsRealTime.IsEnabled(url.ActionContext.HttpContext.RequestServices) ? url.Content("~" + WorkflowsRealTime.HubPath) : null,
            Translations = localizers.GetMergedLocalizations(WorkflowsDesignerJSLocalizer.Group),
        };

        return JsonSerializer.Serialize(config, JOptions.CamelCase);
    }

    // The JSON endpoints the app calls; those a mode doesn't use are null.
    private sealed class DesignerUrls
    {
        public string Definition { get; init; }

        public string Library { get; init; }

        public string Save { get; init; }

        public string AddActivity { get; init; }

        public string Editor { get; init; }

        public string Settings { get; init; }

        public string Publish { get; init; }

        public string Discard { get; init; }

        public string Versions { get; init; }

        public string Restore { get; init; }

        public string Compare { get; init; }

        public string Variables { get; init; }

        public string OutputBindings { get; init; }

        public string Retry { get; init; }

        public string JournalData { get; init; }
    }
}
