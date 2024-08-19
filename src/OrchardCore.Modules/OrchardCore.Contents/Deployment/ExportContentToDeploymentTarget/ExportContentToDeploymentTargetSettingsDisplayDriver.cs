using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

public sealed class ExportContentToDeploymentTargetSettingsDisplayDriver : SiteDisplayDriver<ExportContentToDeploymentTargetSettings>
{
    public const string GroupId = "ExportContentToDeploymentTarget";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ExportContentToDeploymentTargetSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ExportContentToDeploymentTargetSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, OrchardCore.Deployment.CommonPermissions.ManageDeploymentPlan))
        {
            return null;
        }

        return Initialize<ExportContentToDeploymentTargetSettingsViewModel>("ExportContentToDeploymentTargetSettings_Edit", model =>
        {
            model.ExportContentToDeploymentTargetPlanId = settings.ExportContentToDeploymentTargetPlanId;
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ExportContentToDeploymentTargetSettings settings, UpdateEditorContext context)
    {
        var model = new ExportContentToDeploymentTargetSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ExportContentToDeploymentTargetPlanId);

        settings.ExportContentToDeploymentTargetPlanId = model.ExportContentToDeploymentTargetPlanId;

        return await EditAsync(site, settings, context);
    }
}
