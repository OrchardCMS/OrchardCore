using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.WorkflowPruning.Models;
using OrchardCore.Workflows.WorkflowPruning.ViewModels;

namespace OrchardCore.Workflows.WorkflowPruning.Drivers;

public sealed class WorkflowPruningDisplayDriver : SiteDisplayDriver<WorkflowPruningSettings>
{
    public const string GroupId = "WorkflowPruningSettings";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public WorkflowPruningDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override IDisplayResult Edit(ISite site, WorkflowPruningSettings settings, BuildEditorContext context)
    {
        return Initialize<WorkflowPruningViewModel>("WorkflowPruning_Fields_Edit", model =>
        {
            model.RetentionDays = settings.RetentionDays;
            model.LastRunUtc = settings.LastRunUtc;
            model.Disabled = settings.Disabled;
            model.Statuses =
            settings.Statuses ?? new WorkflowStatus[]
            {
                WorkflowStatus.Idle,
                WorkflowStatus.Starting,
                WorkflowStatus.Resuming,
                WorkflowStatus.Executing,
                WorkflowStatus.Halted,
                WorkflowStatus.Finished,
                WorkflowStatus.Faulted,
                WorkflowStatus.Aborted
            };
        }).Location("Content:5")
        .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, WorkflowPruningSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageWorkflowSettings))
        {
            return null;
        }

        var viewModel = new WorkflowPruningViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        settings.RetentionDays = viewModel.RetentionDays;
        settings.Disabled = viewModel.Disabled;
        settings.Statuses = viewModel.Statuses;

        return await EditAsync(site, settings, context);
    }
}
