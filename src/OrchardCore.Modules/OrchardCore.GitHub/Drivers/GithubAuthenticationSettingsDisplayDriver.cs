using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.GitHub.Settings;
using OrchardCore.GitHub.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Drivers;

public sealed class GitHubAuthenticationSettingsDisplayDriver : SiteDisplayDriver<GitHubAuthenticationSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GitHubAuthenticationSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override string SettingsGroupId
        => GitHubConstants.Features.GitHubAuthentication;

    public override async Task<IDisplayResult> EditAsync(ISite site, GitHubAuthenticationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGitHubAuthentication))
        {
            return null;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return Initialize<GitHubAuthenticationSettingsViewModel>("GitHubAuthenticationSettings_Edit", model =>
        {
            model.ClientID = settings.ClientID;
            model.ClientSecretSecretName = settings.ClientSecretSecretName;
            model.HasClientSecret = !string.IsNullOrWhiteSpace(settings.ClientSecret);
            if (settings.CallbackPath.HasValue)
            {
                model.CallbackUrl = settings.CallbackPath.Value;
            }
            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, GitHubAuthenticationSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGitHubAuthentication))
        {
            return null;
        }

        var model = new GitHubAuthenticationSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.ClientID = model.ClientID;
        settings.ClientSecretSecretName = model.ClientSecretSecretName;
        settings.CallbackPath = model.CallbackUrl;
        settings.SaveTokens = model.SaveTokens;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
