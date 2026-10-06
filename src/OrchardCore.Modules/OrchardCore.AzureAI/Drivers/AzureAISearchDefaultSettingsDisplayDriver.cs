using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Options;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.AzureAI.Models;
using OrchardCore.AzureAI.Services;
using OrchardCore.AzureAI.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.AzureAI.Drivers;

public sealed class AzureAISearchDefaultSettingsDisplayDriver : SiteDisplayDriver<AzureAISearchDefaultSettings>
{
    public const string GroupId = "azureAISearch";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IOptionsMonitor<AzureAISearchDefaultOptions> _searchOptions;
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => GroupId;

    public AzureAISearchDefaultSettingsDisplayDriver(
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsMonitor<AzureAISearchDefaultOptions> searchOptions,
        IStringLocalizer<AzureAISearchDefaultSettingsDisplayDriver> stringLocalizer)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _searchOptions = searchOptions;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, AzureAISearchDefaultSettings settings, BuildEditorContext context)
    {
        var searchOptions = _searchOptions.CurrentValue;

        if (searchOptions.DisableUIConfiguration)
        {
            return null;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return Initialize<AzureAISearchDefaultSettingsViewModel>("AzureAISearchDefaultSettings_Edit", model =>
        {
            model.AuthenticationTypes =
            [
                new SelectListItem(S["Default"], nameof(AzureAIAuthenticationType.Default)),
                new SelectListItem(S["Managed Identity"], nameof(AzureAIAuthenticationType.ManagedIdentity)),
                new SelectListItem(S["API Key"], nameof(AzureAIAuthenticationType.ApiKey)),
            ];

            model.ConfigurationsAreOptional = searchOptions.FileConfigurationExists();
            model.AuthenticationType = settings.AuthenticationType;
            model.UseCustomConfiguration = settings.UseCustomConfiguration;
            model.Endpoint = settings.Endpoint;
            model.IdentityClientId = settings.IdentityClientId;
            model.ApiKeySecretName = settings.ApiKeySecretName;
            model.ApiKeyExists = !string.IsNullOrEmpty(settings.ApiKey);
        }).Location("Content")
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext.User, AzureAISearchPermissions.ManageAzureAISearchSettings), this)
        .OnGroup(SettingsGroupId);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureAISearchDefaultSettings settings, UpdateEditorContext context)
    {
        var searchOptions = _searchOptions.CurrentValue;

        if (searchOptions.DisableUIConfiguration)
        {
            return null;
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AzureAISearchPermissions.ManageAzureAISearchSettings))
        {
            return null;
        }

        var model = new AzureAISearchDefaultSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (!searchOptions.FileConfigurationExists())
        {
            model.UseCustomConfiguration = true;
        }

        var useCustomConfigurationChanged = settings.UseCustomConfiguration != model.UseCustomConfiguration;
        var apiKeyChanged = settings.ApiKeySecretName != model.ApiKeySecretName;

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
                settings.ApiKeySecretName = model.ApiKeySecretName;

#pragma warning disable CS0618 // Type or member is obsolete
                // Require either a secret or legacy API key.
                if (string.IsNullOrWhiteSpace(model.ApiKeySecretName) && string.IsNullOrEmpty(settings.ApiKey))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.ApiKeySecretName), S["API Key is required when using API Key authentication type."]);
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        settings.UseCustomConfiguration = model.UseCustomConfiguration;

        if (context.Updater.ModelState.IsValid &&
            (apiKeyChanged ||
             searchOptions.Endpoint != settings.Endpoint ||
             searchOptions.AuthenticationType != settings.AuthenticationType ||
             searchOptions.IdentityClientId != settings.IdentityClientId ||
             useCustomConfigurationChanged))
        {
            _optionsUpdateNotifier.RequestUpdate<AzureAISearchDefaultOptions>();
        }

        return Edit(site, settings, context);
    }
}
