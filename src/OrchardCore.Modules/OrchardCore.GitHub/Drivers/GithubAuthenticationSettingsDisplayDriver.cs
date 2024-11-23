using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    public GitHubAuthenticationSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<GitHubAuthenticationSettingsDisplayDriver> logger)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
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
            if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication);
                    model.ClientSecret = protector.Unprotect(settings.ClientSecret);
                }
                catch (CryptographicException)
                {
                    _logger.LogError("The client secret could not be decrypted. It may have been encrypted using a different key.");
                    model.ClientSecret = string.Empty;
                    model.HasDecryptionError = true;
                }
            }
            else
            {
                model.ClientSecret = string.Empty;
            }
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

        if (context.Updater.ModelState.IsValid)
        {
            var protector = _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication);

            settings.ClientID = model.ClientID;
            settings.ClientSecret = protector.Protect(model.ClientSecret);
            settings.CallbackPath = model.CallbackUrl;
            settings.SaveTokens = model.SaveTokens;

            _shellReleaseManager.RequestRelease();
        }

        return await EditAsync(site, settings, context);
    }
}
