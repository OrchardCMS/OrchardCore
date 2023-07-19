using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Drivers
{
    public class OpenIdClientSettingsDisplayDriver : SectionDisplayDriver<ISite, OpenIdClientSettings>
    {
        private const string SettingsGroupId = "OrchardCore.OpenId.Client";
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOpenIdClientService _clientService;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        protected readonly IStringLocalizer S;

        public OpenIdClientSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IOpenIdClientService clientService,
            IHttpContextAccessor httpContextAccessor,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IStringLocalizer<OpenIdClientSettingsDisplayDriver> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _dataProtectionProvider = dataProtectionProvider;
            _clientService = clientService;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            S = stringLocalizer;
        }

        public override async Task<IDisplayResult> EditAsync(OpenIdClientSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageClientSettings))
            {
                return null;
            }

            return Initialize<OpenIdClientSettingsViewModel>("OpenIdClientSettings_Edit", model =>
            {
                model.DisplayName = settings.DisplayName;
                model.Scopes = settings.Scopes != null ? String.Join(" ", settings.Scopes) : null;
                model.Authority = settings.Authority?.AbsoluteUri;
                model.CallbackPath = settings.CallbackPath;
                model.ClientId = settings.ClientId;
                model.ClientSecret = settings.ClientSecret;
                model.SignedOutCallbackPath = settings.SignedOutCallbackPath;
                model.SignedOutRedirectUri = settings.SignedOutRedirectUri;
                model.ResponseMode = settings.ResponseMode;
                model.StoreExternalTokens = settings.StoreExternalTokens;

                if (settings.ResponseType == OpenIdConnectResponseType.Code)
                {
                    model.UseCodeFlow = true;
                }
                else if (settings.ResponseType == OpenIdConnectResponseType.CodeIdToken)
                {
                    model.UseCodeIdTokenFlow = true;
                }
                else if (settings.ResponseType == OpenIdConnectResponseType.CodeIdTokenToken)
                {
                    model.UseCodeIdTokenTokenFlow = true;
                }
                else if (settings.ResponseType == OpenIdConnectResponseType.CodeToken)
                {
                    model.UseCodeTokenFlow = true;
                }
                else if (settings.ResponseType == OpenIdConnectResponseType.IdToken)
                {
                    model.UseIdTokenFlow = true;
                }
                else if (settings.ResponseType == OpenIdConnectResponseType.IdTokenToken)
                {
                    model.UseIdTokenTokenFlow = true;
                }

                model.Parameters = JsonConvert.SerializeObject(settings.Parameters, _jsonSerializerSettings);
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(OpenIdClientSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageClientSettings))
            {
                return null;
            }

            if (context.GroupId == SettingsGroupId)
            {
                var previousClientSecret = settings.ClientSecret;
                var model = new OpenIdClientSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                model.Scopes ??= String.Empty;

                settings.DisplayName = model.DisplayName;
                settings.Scopes = model.Scopes.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                settings.Authority = !String.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;
                settings.CallbackPath = model.CallbackPath;
                settings.ClientId = model.ClientId;
                settings.SignedOutCallbackPath = model.SignedOutCallbackPath;
                settings.SignedOutRedirectUri = model.SignedOutRedirectUri;
                settings.ResponseMode = model.ResponseMode;
                settings.StoreExternalTokens = model.StoreExternalTokens;

                var useClientSecret = true;

                if (model.UseCodeFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.Code;
                }
                else if (model.UseCodeIdTokenFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                }
                else if (model.UseCodeIdTokenTokenFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.CodeIdTokenToken;
                }
                else if (model.UseCodeTokenFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.CodeToken;
                }
                else if (model.UseIdTokenFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.IdToken;
                    useClientSecret = false;
                }
                else if (model.UseIdTokenTokenFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.IdTokenToken;
                    useClientSecret = false;
                }
                else
                {
                    settings.ResponseType = OpenIdConnectResponseType.None;
                    useClientSecret = false;
                }

                try
                {
                    settings.Parameters = String.IsNullOrWhiteSpace(model.Parameters)
                        ? Array.Empty<ParameterSetting>()
                        : JsonConvert.DeserializeObject<ParameterSetting[]>(model.Parameters);
                }
                catch
                {
                    context.Updater.ModelState.AddModelError(Prefix, S["The parameters are written in an incorrect format."]);
                }

                if (!useClientSecret)
                {
                    model.ClientSecret = previousClientSecret = null;
                }

                // Restore the client secret if the input is empty (i.e if it hasn't been reset).
                if (String.IsNullOrEmpty(model.ClientSecret))
                {
                    settings.ClientSecret = previousClientSecret;
                }
                else
                {
                    var protector = _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration));
                    settings.ClientSecret = protector.Protect(model.ClientSecret);
                }

                foreach (var result in await _clientService.ValidateSettingsAsync(settings))
                {
                    if (result != ValidationResult.Success)
                    {
                        var key = result.MemberNames.FirstOrDefault() ?? String.Empty;
                        context.Updater.ModelState.AddModelError(key, result.ErrorMessage);
                    }
                }

                // If the settings are valid, release the current tenant.
                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(settings, context);
        }
    }
}
