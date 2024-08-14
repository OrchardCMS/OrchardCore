using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Trimming.Models;
using OrchardCore.Workflows.Trimming.ViewModels;

namespace OrchardCore.Workflows.Trimming.Drivers;

public sealed class WorkflowTrimmingDisplayDriver : SiteDisplayDriver<WorkflowTrimmingSettings>
{
    public const string GroupId = "WorkflowTrimmingSettings";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public WorkflowTrimmingDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override IDisplayResult Edit(ISite site, WorkflowTrimmingSettings settings, BuildEditorContext context)
    {
        return Initialize<WorkflowTrimmingViewModel>("WorkflowTrimming_Fields_Edit", model =>
        {
            model.RetentionDays = settings.RetentionDays;
            model.LastRunUtc = settings.LastRunUtc;
            model.Disabled = settings.Disabled;
            model.Statuses = settings.Statuses ??
            [
                WorkflowStatus.Idle,
                WorkflowStatus.Starting,
                WorkflowStatus.Resuming,
                WorkflowStatus.Executing,
                WorkflowStatus.Halted,
                WorkflowStatus.Finished,
                WorkflowStatus.Faulted,
                WorkflowStatus.Aborted
            ];
        }).Location("Content:5")
        .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, WorkflowTrimmingSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageWorkflowSettings))
        {
            return null;
        }

        var viewModel = new WorkflowTrimmingViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        settings.RetentionDays = viewModel.RetentionDays;
        settings.Disabled = viewModel.Disabled;
        settings.Statuses = viewModel.Statuses;

        return await EditAsync(site, settings, context);
    }
}
