using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Settings;
using static OrchardCore.OpenId.ViewModels.OpenIdServerSettingsViewModel;

namespace OrchardCore.OpenId.Drivers
{
    public class OpenIdServerSettingsDisplayDriver : SectionDisplayDriver<ISite, OpenIdServerSettings>
    {
        private const string SettingsGroupId = "OrchardCore.OpenId.Server";

        private readonly IAuthorizationService _authorizationService;
        private readonly IOpenIdServerService _serverService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<OpenIdServerSettingsDisplayDriver> T;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public OpenIdServerSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IOpenIdServerService serverService,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IHtmlLocalizer<OpenIdServerSettingsDisplayDriver> stringLocalizer,
            IShellHost shellHost,
            ShellSettings shellSettings)
        {
            _authorizationService = authorizationService;
            _serverService = serverService;
            _notifier = notifier;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            T = stringLocalizer;
        }

        public override async Task<IDisplayResult> EditAsync(OpenIdServerSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageServerSettings))
            {
                return null;
            }

            return Initialize<OpenIdServerSettingsViewModel>("OpenIdServerSettings_Edit", async model =>
            {
                model.TestingModeEnabled = settings.TestingModeEnabled;
                model.AccessTokenFormat = settings.AccessTokenFormat;
                model.Authority = settings.Authority;
                model.CertificateStoreLocation = settings.CertificateStoreLocation;
                model.CertificateStoreName = settings.CertificateStoreName;
                model.CertificateThumbprint = settings.CertificateThumbprint;
                model.EnableTokenEndpoint = settings.EnableTokenEndpoint;
                model.EnableAuthorizationEndpoint = settings.EnableAuthorizationEndpoint;
                model.EnableLogoutEndpoint = settings.EnableLogoutEndpoint;
                model.EnableUserInfoEndpoint = settings.EnableUserInfoEndpoint;
                model.AllowPasswordFlow = settings.AllowPasswordFlow;
                model.AllowClientCredentialsFlow = settings.AllowClientCredentialsFlow;
                model.AllowAuthorizationCodeFlow = settings.AllowAuthorizationCodeFlow;
                model.AllowRefreshTokenFlow = settings.AllowRefreshTokenFlow;
                model.AllowImplicitFlow = settings.AllowImplicitFlow;
                model.UseRollingTokens = settings.UseRollingTokens;

                foreach (var (certificate, location, name) in await _serverService.GetAvailableCertificatesAsync())
                {
                    model.AvailableCertificates.Add(new CertificateInfo
                    {
                        StoreLocation = location,
                        StoreName = name,
                        FriendlyName = certificate.FriendlyName,
                        Issuer = certificate.Issuer,
                        Subject = certificate.Subject,
                        NotBefore = certificate.NotBefore,
                        NotAfter = certificate.NotAfter,
                        ThumbPrint = certificate.Thumbprint,
                        HasPrivateKey = certificate.HasPrivateKey,
                        Archived = certificate.Archived
                    });
                }
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(OpenIdServerSettings settings,  BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageServerSettings))
            {
                return null;
            }

            if (context.GroupId == SettingsGroupId)
            {
                var model = new OpenIdServerSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.TestingModeEnabled = model.TestingModeEnabled;
                settings.AccessTokenFormat = model.AccessTokenFormat;
                settings.Authority = model.Authority;
                settings.CertificateStoreLocation = model.CertificateStoreLocation;
                settings.CertificateStoreName = model.CertificateStoreName;
                settings.CertificateThumbprint = model.CertificateThumbprint;
                settings.EnableTokenEndpoint = model.EnableTokenEndpoint;
                settings.EnableAuthorizationEndpoint = model.EnableAuthorizationEndpoint;
                settings.EnableLogoutEndpoint = model.EnableLogoutEndpoint;
                settings.EnableUserInfoEndpoint = model.EnableUserInfoEndpoint;
                settings.AllowPasswordFlow = model.AllowPasswordFlow;
                settings.AllowClientCredentialsFlow = model.AllowClientCredentialsFlow;
                settings.AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow;
                settings.AllowRefreshTokenFlow = model.AllowRefreshTokenFlow;
                settings.AllowImplicitFlow = model.AllowImplicitFlow;
                settings.UseRollingTokens = model.UseRollingTokens;

                foreach (var result in await _serverService.ValidateSettingsAsync(settings))
                {
                    if (result != ValidationResult.Success)
                    {
                        var key = result.MemberNames.FirstOrDefault() ?? string.Empty;
                        context.Updater.ModelState.AddModelError(key, result.ErrorMessage);
                    }
                }

                // If the settings are valid, reload the current tenant.
                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(settings, context);
        }
    }
}
