using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
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

    internal readonly IStringLocalizer S;

    public ExportContentToDeploymentTargetSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<ExportContentToDeploymentTargetSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ExportContentToDeploymentTargetSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, DeploymentPermissions.ManageDeploymentPlan))
        {
            return null;
        }

        return Initialize<ExportContentToDeploymentTargetSettingsViewModel>("ExportContentToDeploymentTargetSettings_Edit", model =>
        {
            model.ExportContentToDeploymentTargetPlanId = settings.ExportContentToDeploymentTargetPlanId;
        }).Location(l => l.Zone("Content", "2").Card(S["Deployment plan"], "10"))
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
