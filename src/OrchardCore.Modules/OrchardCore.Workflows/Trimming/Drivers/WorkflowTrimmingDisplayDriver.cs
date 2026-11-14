using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
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

    internal readonly IStringLocalizer S;

    public WorkflowTrimmingDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IDocumentManager<WorkflowTrimmingState> workflowTrimmingStateDocumentManager,
        IStringLocalizer<WorkflowTrimmingDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _workflowTrimmingStateDocumentManager = workflowTrimmingStateDocumentManager;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite model, WorkflowTrimmingSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, WorkflowsPermissions.ManageWorkflowSettings))
        {
            return null;
        }

        return Combine(
            Initialize<WorkflowTrimmingViewModel>("WorkflowTrimming_Fields_Edit", model =>
            {
                model.RetentionDays = settings.RetentionDays;

                foreach (var status in settings.Statuses ?? [])
                {
                    model.Statuses.Single(statusItem => statusItem.Status == status).IsSelected = true;
                }
            }).Location(l => l.Zone("Content", "5").Card(S["Retention"], "10"))
            .OnGroup(GroupId),
            Initialize<WorkflowTrimmingViewModel>("WorkflowTrimmingTask_Fields_Edit", async model =>
            {
                model.Disabled = settings.Disabled;
                model.LastRunUtc = (await _workflowTrimmingStateDocumentManager.GetOrCreateImmutableAsync()).LastRunUtc;
            }).Location(l => l.Zone("Content", "6").Card(S["Background task"], "20"))
            .OnGroup(GroupId)
        );
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, WorkflowTrimmingSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, WorkflowsPermissions.ManageWorkflowSettings))
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
