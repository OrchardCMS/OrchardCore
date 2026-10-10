using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.GitHub.Settings;
using OrchardCore.GitHub.ViewModels;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Drivers;

public sealed class GitHubAuthenticationSettingsDisplayDriver : SiteDisplayDriver<GitHubAuthenticationSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;

    public GitHubAuthenticationSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
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

        return Initialize<GitHubAuthenticationSettingsViewModel>("GitHubAuthenticationSettings_Edit", model =>
        {
            model.ClientID = settings.ClientID;
            model.ClientSecret = SecretInputViewModel.Create(settings.ClientSecret, settings.ClientSecretSecretName);
            if (settings.CallbackPath.HasValue)
            {
                model.CallbackUrl = settings.CallbackPath.Value;
            }
            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
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

        var clientSecret = await model.ClientSecret.UpdateAsync(new SecretInputUpdateContext(
            _serviceProvider,
            _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication),
            context.Updater.ModelState,
            $"{Prefix}.{nameof(model.ClientSecret)}")
        {
            ProtectedValue = settings.ClientSecret,
            SecretName = settings.ClientSecretSecretName,
        });

        if (clientSecret.Succeeded)
        {
            settings.ClientSecret = clientSecret.ProtectedValue;
            settings.ClientSecretSecretName = clientSecret.SecretName;
        }

        settings.ClientID = model.ClientID;
        settings.CallbackPath = model.CallbackUrl;
        settings.SaveTokens = model.SaveTokens;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
