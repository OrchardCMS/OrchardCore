using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.GitHub.Settings;
using OrchardCore.GitHub.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Drivers
{
    public class GitHubAuthenticationSettingsDisplayDriver : SectionDisplayDriver<ISite, GitHubAuthenticationSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public GitHubAuthenticationSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IHttpContextAccessor httpContextAccessor,
            IShellHost shellHost,
            ShellSettings shellSettings)
        {
            _authorizationService = authorizationService;
            _dataProtectionProvider = dataProtectionProvider;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override async Task<IDisplayResult> EditAsync(GitHubAuthenticationSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageGitHubAuthentication))
            {
                return null;
            }

            return Initialize<GitHubAuthenticationSettingsViewModel>("GitHubAuthenticationSettings_Edit", model =>
            {
                model.ClientID = settings.ClientID;
                if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
                {
                    var protector = _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication);
                    model.ClientSecret = protector.Unprotect(settings.ClientSecret);
                }
                else
                {
                    model.ClientSecret = string.Empty;
                }
                if (settings.CallbackPath.HasValue)
                {
                    model.CallbackUrl = settings.CallbackPath.Value;
                }
            }).Location("Content:5").OnGroup(GitHubConstants.Features.GitHubAuthentication);
        }

        public override async Task<IDisplayResult> UpdateAsync(GitHubAuthenticationSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == GitHubConstants.Features.GitHubAuthentication)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageGitHubAuthentication))
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
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }
            return await EditAsync(settings, context);
        }
    }
}
