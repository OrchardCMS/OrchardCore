using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace OrchardCore.Workflows.Controllers;

/// <summary>
/// The URLs of the activity pages the designer replaced. They redirect to the designer, so bookmarks and
/// links to them keep working.
/// </summary>
[Admin]
public sealed class ActivityController : Controller
{
    private const string Area = "OrchardCore.Workflows";

    [Admin("Workflows/Types/{workflowTypeId}/Activity/{activityName}/Add", "AddActivity")]
    public IActionResult Create(long workflowTypeId)
        => RedirectToAction("Edit", "WorkflowType", new { area = Area, id = workflowTypeId });

    [Admin("Workflows/Types/{workflowTypeId}/Activity/{activityId}/Edit", "EditActivity")]
    public IActionResult Edit(long workflowTypeId, string activityId)
        => RedirectToAction("Edit", "WorkflowType", new { area = Area, id = workflowTypeId, activityId });
}
