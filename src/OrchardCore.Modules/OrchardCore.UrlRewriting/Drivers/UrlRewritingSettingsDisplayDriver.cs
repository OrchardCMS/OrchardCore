using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.ViewModels;
using OrchardCore.Settings;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.UrlRewriting.Drivers;

internal sealed class UrlRewritingSettingsDisplayDriver : SiteDisplayDriver<UrlRewritingSettings>
{
    public const string GroupId = "UrlRewriting";

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IShellReleaseManager _shellReleaseManager;

    internal readonly IStringLocalizer S;

    public UrlRewritingSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IShellReleaseManager shellReleaseManager,
        IStringLocalizer<UrlRewritingSettingsDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _shellReleaseManager = shellReleaseManager;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, UrlRewritingSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<UrlRewritingSettingsViewModel>("UrlRewritingSettings_Edit", model =>
        {
            model.ApacheModRewrite = settings.ApacheModRewrite;
        }).Location("Content:1")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, UrlRewritingSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return null;
        }

        var model = new UrlRewritingSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.ApacheModRewrite = model.ApacheModRewrite;

        ValidateUrls(settings, context.Updater);

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }

    private void ValidateUrls(UrlRewritingSettings settings, IUpdateModel updater)
    {
        try
        {
            var rewriteOptions = new RewriteOptions();
            using var apacheModRewrite = new StringReader(settings.ApacheModRewrite);
            rewriteOptions.AddApacheModRewrite(apacheModRewrite);
        }
        catch (FormatException ex)
        {
            updater.ModelState.AddModelError(Prefix, nameof(UrlRewritingSettingsViewModel.ApacheModRewrite), S["Parsing error: {0}", ex.Message]);
        }
        catch (NotImplementedException ex)
        {
            updater.ModelState.AddModelError(Prefix, nameof(UrlRewritingSettingsViewModel.ApacheModRewrite), S["Parsing error: {0}", ex.Message]);
        }
    }
}
