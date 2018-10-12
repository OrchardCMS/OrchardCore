using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Settings;
using static OpenIddict.Abstractions.OpenIddictConstants;
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

                model.EnableAuthorizationEndpoint = settings.AuthorizationEndpointPath.HasValue;
                model.EnableLogoutEndpoint = settings.LogoutEndpointPath.HasValue;
                model.EnableTokenEndpoint = settings.TokenEndpointPath.HasValue;
                model.EnableUserInfoEndpoint = settings.UserinfoEndpointPath.HasValue;

                model.AllowPasswordFlow = settings.GrantTypes.Contains(GrantTypes.Password);
                model.AllowClientCredentialsFlow = settings.GrantTypes.Contains(GrantTypes.ClientCredentials);
                model.AllowAuthorizationCodeFlow = settings.GrantTypes.Contains(GrantTypes.AuthorizationCode);
                model.AllowRefreshTokenFlow = settings.GrantTypes.Contains(GrantTypes.RefreshToken);
                model.AllowImplicitFlow = settings.GrantTypes.Contains(GrantTypes.Implicit);

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

                settings.AuthorizationEndpointPath = model.EnableAuthorizationEndpoint ?
                    new PathString("/connect/authorize") : PathString.Empty;
                settings.LogoutEndpointPath = model.EnableLogoutEndpoint ?
                    new PathString("/connect/logout") : PathString.Empty;
                settings.TokenEndpointPath = model.EnableTokenEndpoint ?
                    new PathString("/connect/token") : PathString.Empty;
                settings.UserinfoEndpointPath = model.EnableUserInfoEndpoint ?
                    new PathString("/connect/userinfo") : PathString.Empty;

                if (model.AllowAuthorizationCodeFlow)
                {
                    settings.GrantTypes.Add(GrantTypes.AuthorizationCode);
                }
                else
                {
                    settings.GrantTypes.Remove(GrantTypes.AuthorizationCode);
                }

                if (model.AllowImplicitFlow)
                {
                    settings.GrantTypes.Add(GrantTypes.Implicit);
                }
                else
                {
                    settings.GrantTypes.Remove(GrantTypes.Implicit);
                }

                if (model.AllowClientCredentialsFlow)
                {
                    settings.GrantTypes.Add(GrantTypes.ClientCredentials);
                }
                else
                {
                    settings.GrantTypes.Remove(GrantTypes.ClientCredentials);
                }

                if (model.AllowPasswordFlow)
                {
                    settings.GrantTypes.Add(GrantTypes.Password);
                }
                else
                {
                    settings.GrantTypes.Remove(GrantTypes.Password);
                }

                if (model.AllowRefreshTokenFlow)
                {
                    settings.GrantTypes.Add(GrantTypes.RefreshToken);
                }
                else
                {
                    settings.GrantTypes.Remove(GrantTypes.RefreshToken);
                }

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
