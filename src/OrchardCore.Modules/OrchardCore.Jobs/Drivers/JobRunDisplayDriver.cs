using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Jobs.Models;
using OrchardCore.Jobs.Services;
using OrchardCore.Jobs.ViewModels;

namespace OrchardCore.Jobs.Drivers;

/// <summary>
/// Builds the row of the runs admin list. Each shape is placed in a zone of the row for the List layout, and in
/// the zone of a column of <c>JobRuns</c> for the Grid layout.
/// </summary>
public sealed class JobRunDisplayDriver : DisplayDriver<JobRun>
{
    private readonly JobDisplayService _displayService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool? _canManage;

    public JobRunDisplayDriver(
        JobDisplayService displayService,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _displayService = displayService;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IDisplayResult> DisplayAsync(JobRun run, BuildDisplayContext context)
    {
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape rowShape)
        {
            // Rendered on the row element in every layout, so that the page can follow the run.
            rowShape.Attributes["data-job-run-id"] = run.RunId;
            rowShape.Attributes["data-job-run-signature"] = JobDisplayService.GetSignature(run);
            rowShape.Attributes["data-job-run-active"] = run.IsCompleted ? "false" : "true";
        }

        _canManage ??= await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, JobsPermissions.ManageJobs);

        var inGrid = context.DisplayType == OrchardCoreConstants.DisplayType.SummaryAdminGrid;

        void Populate(JobRunRowViewModel model)
        {
            model.Run = run;
            model.JobTitle = _displayService.GetTitle(run.JobName);
            model.IsJobRegistered = _displayService.Find(run.JobName) is not null;
            model.CanManage = _canManage.Value;
            model.InGrid = inGrid;
        }

        var results = new List<IDisplayResult>
        {
            Initialize<JobRunRowViewModel>("JobRun_Fields_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Title:1"),
            Initialize<JobRunRowViewModel>("JobRun_Description_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Title:5"),
            Initialize<JobRunRowViewModel>("JobRun_Status_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Status:5"),
            Initialize<JobRunRowViewModel>("JobRun_Trigger_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Trigger:5"),
            Initialize<JobRunRowViewModel>("JobRun_Started_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Started:5"),
            Initialize<JobRunRowViewModel>("JobRun_Duration_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Duration:5"),
            Initialize<JobRunRowViewModel>("JobRun_Node_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:15")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Server:5"),
            Initialize<JobRunRowViewModel>("JobRun_Buttons_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
        };

        if (_canManage.Value)
        {
            results.Add(Initialize<JobRunRowViewModel>("JobRun_Checkbox_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"));
            results.Add(Initialize<JobRunRowViewModel>("JobRun_ActionsMenuItems_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:5"));
        }

        return Combine(results);
    }
}
