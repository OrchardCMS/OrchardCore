using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Documents;
using OrchardCore.Settings;
using OrchardCore.Workflows.Trimming.Models;
using OrchardCore.Workflows.Trimming.ViewModels;

namespace OrchardCore.Workflows.Trimming.Drivers;

public sealed class WorkflowTrimmingDisplayDriver : SiteDisplayDriver<WorkflowTrimmingSettings>
{
    public const string GroupId = "WorkflowTrimmingSettings";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentManager<WorkflowTrimmingState> _workflowTrimmingStateDocumentManager;

    public WorkflowTrimmingDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IDocumentManager<WorkflowTrimmingState> workflowTrimmingStateDocumentManager)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _workflowTrimmingStateDocumentManager = workflowTrimmingStateDocumentManager;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite model, WorkflowTrimmingSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageWorkflowSettings))
        {
            return null;
        }

        return Initialize<WorkflowTrimmingViewModel>("WorkflowTrimming_Fields_Edit", async model =>
        {
            model.RetentionDays = settings.RetentionDays;
            model.LastRunUtc = (await _workflowTrimmingStateDocumentManager.GetOrCreateImmutableAsync()).LastRunUtc;
            model.Disabled = settings.Disabled;

            foreach (var status in settings.Statuses ?? [])
            {
                model.Statuses.Single(statusItem => statusItem.Status == status).IsSelected = true;
            }
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
        settings.Statuses = viewModel.Statuses
            .Where(statusItem => statusItem.IsSelected)
            .Select(statusItem => statusItem.Status)
            .ToArray();

        return await EditAsync(site, settings, context);
    }
}
