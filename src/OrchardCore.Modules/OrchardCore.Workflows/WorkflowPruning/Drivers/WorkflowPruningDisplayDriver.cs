using System.Threading.Tasks;
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

public class WorkflowPruningDisplayDriver
 : SiteDisplayDriver< WorkflowPruningSettings>
{
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

    public const string GroupId = "WorkflowPruningSettings";

    protected override string SettingsGroupId => GroupId;

    public override IDisplayResult Edit(ISite model, WorkflowPruningSettings section, BuildEditorContext context)
    {
        return Initialize<WorkflowPruningViewModel>(
                "WorkflowPruning_Fields_Edit",
                model =>
                {
                    model.RetentionDays = section.RetentionDays;
                    model.LastRunUtc = section.LastRunUtc;
                    model.Disabled = section.Disabled;
                    model.Statuses =
                        section.Statuses
                        ?? new WorkflowStatus[]
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
                }
            )
            .Location("Content:5")
            .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(
        ISite model,
        WorkflowPruningSettings section,
        UpdateEditorContext context
    )
    {
        if (
            !await _authorizationService.AuthorizeAsync(
                _httpContextAccessor.HttpContext?.User,
                Permissions.ManageWorkflowSettings
            )
        )
        {
            return null;
        }

        if (context.GroupId == GroupId)
        {
            var viewModel = new WorkflowPruningViewModel();
            await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

            section.RetentionDays = viewModel.RetentionDays;
            section.Disabled = viewModel.Disabled;
            section.Statuses = viewModel.Statuses;
        }

        return await EditAsync(model, section, context);
    }
}
