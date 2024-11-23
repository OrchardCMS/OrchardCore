using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class AzureAISearchDefaultSettingsDisplayDriver : SiteDisplayDriver<AzureAISearchDefaultSettings>
{
    public const string GroupId = "azureAISearch";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly AzureAISearchDefaultOptions _searchOptions;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IShellReleaseManager _shellReleaseManager;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => GroupId;

    public AzureAISearchDefaultSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptions<AzureAISearchDefaultOptions> searchOptions,
        IDataProtectionProvider dataProtectionProvider,
        IStringLocalizer<AzureAISearchDefaultSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _searchOptions = searchOptions.Value;
        _dataProtectionProvider = dataProtectionProvider;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, AzureAISearchDefaultSettings settings, BuildEditorContext context)
    {
        if (_searchOptions.DisableUIConfiguration)
        {
            return null;
        }

        return Initialize<AzureAISearchDefaultSettingsViewModel>("AzureAISearchDefaultSettings_Edit", model =>
        {
            model.AuthenticationTypes =
            [
                new SelectListItem(S["Default"], nameof(AzureAIAuthenticationType.Default)),
                new SelectListItem(S["Managed Identity"], nameof(AzureAIAuthenticationType.ManagedIdentity)),
                new SelectListItem(S["API Key"], nameof(AzureAIAuthenticationType.ApiKey)),
            ];

            model.ConfigurationsAreOptional = _searchOptions.FileConfigurationExists();
            model.AuthenticationType = settings.AuthenticationType;
            model.UseCustomConfiguration = settings.UseCustomConfiguration;
            model.Endpoint = settings.Endpoint;
            model.IdentityClientId = settings.IdentityClientId;
            model.ApiKeyExists = !string.IsNullOrEmpty(settings.ApiKey);
        }).Location("Content")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureAISearchDefaultSettings settings, UpdateEditorContext context)
    {
        if (_searchOptions.DisableUIConfiguration)
        {
            return null;
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return null;
        }

        var model = new AzureAISearchDefaultSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (!_searchOptions.FileConfigurationExists())
        {
            model.UseCustomConfiguration = true;
        }

        var useCustomConfigurationChanged = settings.UseCustomConfiguration != model.UseCustomConfiguration;

        if (model.UseCustomConfiguration)
        {
            settings.AuthenticationType = model.AuthenticationType.Value;
            settings.Endpoint = model.Endpoint;
            settings.IdentityClientId = model.IdentityClientId?.Trim();

            if (string.IsNullOrWhiteSpace(model.Endpoint))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Endpoint), S["Endpoint is required."]);
            }
            else if (!Uri.TryCreate(model.Endpoint, UriKind.Absolute, out var _))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Endpoint), S["Endpoint must be a valid url."]);
            }

            if (model.AuthenticationType == AzureAIAuthenticationType.ApiKey)
            {
                var hasNewKey = !string.IsNullOrWhiteSpace(model.ApiKey);

                if (!hasNewKey && string.IsNullOrEmpty(settings.ApiKey))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.ApiKey), S["API Key is required when using API Key authentication type."]);
                }
                else if (hasNewKey)
                {
                    var protector = _dataProtectionProvider.CreateProtector(AzureAISearchDefaultOptionsConfigurations.ProtectorName);

                    settings.ApiKey = protector.Protect(model.ApiKey);
                }
            }
        }

        settings.UseCustomConfiguration = model.UseCustomConfiguration;

        if (context.Updater.ModelState.IsValid &&
            (_searchOptions.Credential?.Key != model.ApiKey ||
             _searchOptions.Endpoint != settings.Endpoint ||
             _searchOptions.AuthenticationType != settings.AuthenticationType ||
             _searchOptions.IdentityClientId != settings.IdentityClientId ||
             useCustomConfigurationChanged))
        {
            _shellReleaseManager.RequestRelease();
        }

        return Edit(site, settings, context);
    }
}
