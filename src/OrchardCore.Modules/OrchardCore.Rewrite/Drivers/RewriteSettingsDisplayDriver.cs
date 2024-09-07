using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Rewrite.Models;
using OrchardCore.Rewrite.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Rewrite.Drivers;

internal class RewriteSettingsDisplayDriver : SiteDisplayDriver<RewriteSettings>
{
    public const string GroupId = "Rewrite";

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IStringLocalizer S;

    public RewriteSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IShellReleaseManager shellReleaseManager,
        IStringLocalizer<RewriteSettingsDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _shellReleaseManager = shellReleaseManager;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, RewriteSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageRewrites))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<RewriteSettingsViewModel>("RewriteSettings_Edit", model =>
        {
            model.ApacheModRewrite = settings.ApacheModRewrite;
        }).Location("Content:1")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, RewriteSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageRewrites))
        {
            return null;
        }

        var viewModel = new RewriteSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        settings.ApacheModRewrite = viewModel.ApacheModRewrite;

        ValidateUrls(settings, context.Updater);

        if (context.Updater.ModelState.IsValid)
        {
            _shellReleaseManager.RequestRelease();
        }

        return await EditAsync(site, settings, context);
    }

    private void ValidateUrls(RewriteSettings settings, IUpdateModel updater)
    {
        try
        {
            var rewriteOptions = new RewriteOptions();
            using var apacheModRewrite = new StringReader(settings.ApacheModRewrite);
            rewriteOptions.AddApacheModRewrite(apacheModRewrite);
        }
        catch (FormatException ex)
        {
            updater.ModelState.AddModelError(nameof(settings.ApacheModRewrite), "Parsing error: " + ex.Message);
        }
        catch (NotImplementedException ex)
        {
            updater.ModelState.AddModelError(nameof(settings.ApacheModRewrite), "Parsing error: " + ex.Message);
        }
    }
}
