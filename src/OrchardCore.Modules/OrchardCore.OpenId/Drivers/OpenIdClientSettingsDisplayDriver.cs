using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Drivers;

public sealed class OpenIdClientSettingsDisplayDriver : SiteDisplayDriver<OpenIdClientSettings>
{
    private static readonly char[] _separator = [' ', ','];

    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOpenIdClientService _clientService;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => "OrchardCore.OpenId.Client";

    public OpenIdClientSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IOpenIdClientService clientService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<OpenIdClientSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _clientService = clientService;
        _httpContextAccessor = httpContextAccessor;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, OpenIdClientSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageClientSettings))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<OpenIdClientSettingsViewModel>("OpenIdClientSettings_Edit", model =>
        {
            model.DisplayName = settings.DisplayName;
            model.Scopes = settings.Scopes != null ? string.Join(" ", settings.Scopes) : null;
            model.Authority = settings.Authority?.AbsoluteUri;
            model.CallbackPath = settings.CallbackPath;
            model.ClientId = settings.ClientId;
            model.HasClientSecret = !string.IsNullOrEmpty(settings.ClientSecret);
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

            model.Parameters = JConvert.SerializeObject(settings.Parameters, JOptions.CamelCase);
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, OpenIdClientSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageClientSettings))
        {
            return null;
        }

        var previousClientSecret = settings.ClientSecret;
        var model = new OpenIdClientSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        model.Scopes ??= string.Empty;

        settings.DisplayName = model.DisplayName;
        settings.Scopes = model.Scopes.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
        settings.Authority = !string.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;
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
            settings.Parameters = string.IsNullOrWhiteSpace(model.Parameters)
                ? []
                : JConvert.DeserializeObject<ParameterSetting[]>(model.Parameters);
        }
        catch
        {
            context.Updater.ModelState.AddModelError(Prefix, S["The parameters are written in an incorrect format."]);
        }

        if (!useClientSecret)
        {
            model.ClientSecret = previousClientSecret = null;
        }

        if (!string.IsNullOrEmpty(model.ClientSecret))
        {
            var protector = _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration));
            settings.ClientSecret = protector.Protect(model.ClientSecret);
        }

        foreach (var result in await _clientService.ValidateSettingsAsync(settings))
        {
            if (result != ValidationResult.Success)
            {
                var key = result.MemberNames.FirstOrDefault() ?? string.Empty;
                context.Updater.ModelState.AddModelError(key, result.ErrorMessage);
            }
        }

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
