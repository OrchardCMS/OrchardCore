using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Jobs.Models;
using OrchardCore.Jobs.Services;
using OrchardCore.Jobs.ViewModels;

namespace OrchardCore.Jobs.Drivers;

/// <summary>
/// Builds the row of the schedules admin list. Each shape is placed in a zone of the row for the List layout, and
/// in the zone of a column of <c>JobSchedules</c> for the Grid layout.
/// </summary>
public sealed class JobScheduleDisplayDriver : DisplayDriver<JobSchedule>
{
    private readonly JobDisplayService _displayService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool? _canManage;

    public JobScheduleDisplayDriver(
        JobDisplayService displayService,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _displayService = displayService;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IDisplayResult> DisplayAsync(JobSchedule schedule, BuildDisplayContext context)
    {
        _canManage ??= await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, JobsPermissions.ManageJobs);

        var job = _displayService.Find(schedule.JobName);
        var inGrid = context.DisplayType == OrchardCoreConstants.DisplayType.SummaryAdminGrid;

        void Populate(JobScheduleRowViewModel model)
        {
            model.Schedule = schedule;
            model.JobTitle = _displayService.GetTitle(schedule.JobName);
            model.IsJobRegistered = job is not null;
            model.CanManage = _canManage.Value;

            // The schedule a registered job declares would be created again.
            model.CanDelete = !(schedule.IsDeclared && job?.Schedule is not null);
            model.InGrid = inGrid;
        }

        var results = new List<IDisplayResult>
        {
            Initialize<JobScheduleRowViewModel>("JobSchedule_Fields_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            Initialize<JobScheduleRowViewModel>("JobSchedule_Job_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Job:5"),
            Initialize<JobScheduleRowViewModel>("JobSchedule_Cron_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Schedule:5"),
            Initialize<JobScheduleRowViewModel>("JobSchedule_NextRun_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "NextRun:5"),
            Initialize<JobScheduleRowViewModel>("JobSchedule_LastRun_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "LastRun:5"),
        };

        if (_canManage.Value)
        {
            results.Add(Initialize<JobScheduleRowViewModel>("JobSchedule_Buttons_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"));
            results.Add(Initialize<JobScheduleRowViewModel>("JobSchedule_ActionsMenuItems_SummaryAdmin", Populate)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:5"));
        }

        return Combine(results);
    }
}
