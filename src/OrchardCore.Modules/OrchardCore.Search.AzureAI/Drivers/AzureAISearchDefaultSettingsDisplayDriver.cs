using System;
using System.Threading.Tasks;
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
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Drivers;

public class AzureAISearchDefaultSettingsDisplayDriver : SectionDisplayDriver<ISite, AzureAISearchDefaultSettings>
{
    private static readonly char[] _separator = [',', ' '];

    private readonly AzureAISearchIndexSettingsService _indexSettingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ShellSettings _shellSettings;
    private readonly IShellHost _shellHost;
    private readonly AzureAISearchDefaultOptions _searchOptions;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    protected readonly IStringLocalizer S;

    public AzureAISearchDefaultSettingsDisplayDriver(
        AzureAISearchIndexSettingsService indexSettingsService,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptions<AzureAISearchDefaultOptions> searchOptions,
        ShellSettings shellSettings,
        IShellHost shellHost,
        IDataProtectionProvider dataProtectionProvider,
        IStringLocalizer<AzureAISearchDefaultSettingsDisplayDriver> stringLocalizer
        )
    {
        _indexSettingsService = indexSettingsService;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellSettings = shellSettings;
        _shellHost = shellHost;
        _searchOptions = searchOptions.Value;
        _dataProtectionProvider = dataProtectionProvider;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AzureAISearchDefaultSettings settings)
    {
        if (_searchOptions.ConfigurationType == AzureAIConfigurationType.File)
        {
            return null;
        }

        return Initialize<AzureAISearchDefaultSettingsViewModel>("AzureAISearchDefaultSettings_Edit", model =>
        {
            model.ConfigurationsAreOptional = _searchOptions.ConfigurationType == AzureAIConfigurationType.UIThenFile;

            model.AuthenticationTypes = new[]
            {
                new SelectListItem(S["Default"], nameof(AzureAIAuthenticationType.Default)),
                new SelectListItem(S["API Key"], nameof(AzureAIAuthenticationType.ApiKey)),
            };
            model.AuthenticationType = settings.AuthenticationType;
            model.UseCustomConfiguration = settings.UseCustomConfiguration;
            model.Endpoint = settings.Endpoint;
            model.ApiKeyExists = !string.IsNullOrEmpty(settings.ApiKey);

        }).Location("Content:10#Azure AI Search;5")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        .Prefix(Prefix)
        .OnGroup(SearchConstants.SearchSettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchDefaultSettings settings, BuildEditorContext context)
    {
        if (!SearchConstants.SearchSettingsGroupId.EqualsOrdinalIgnoreCase(context.GroupId))
        {
            return null;
        }

        if (_searchOptions.ConfigurationType == AzureAIConfigurationType.File)
        {
            return null;
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return null;
        }

        var model = new AzureAISearchDefaultSettingsViewModel();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            if (_searchOptions.ConfigurationType == AzureAIConfigurationType.UI)
            {
                model.UseCustomConfiguration = true;
            }

            var useCustomConfigurationChanged = settings.UseCustomConfiguration != model.UseCustomConfiguration;

            if (model.UseCustomConfiguration)
            {
                settings.AuthenticationType = model.AuthenticationType.Value;
                settings.Endpoint = model.Endpoint;

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
                        var protector = _dataProtectionProvider.CreateProtector("AzureAISearch");

                        settings.ApiKey = protector.Protect(model.ApiKey);
                    }
                }
            }

            settings.UseCustomConfiguration = model.UseCustomConfiguration;

            if (context.Updater.ModelState.IsValid &&
                (_searchOptions.Credential?.Key != model.ApiKey
                || _searchOptions.Endpoint != settings.Endpoint
                || _searchOptions.AuthenticationType != settings.AuthenticationType
                || useCustomConfigurationChanged))
            {
                await _shellHost.ReleaseShellContextAsync(_shellSettings);
            }
        }

        return Edit(settings);
    }

    protected override void BuildPrefix(ISite model, string htmlFieldPrefix)
    {
        Prefix = typeof(AzureAISearchSettings).Name;

        if (!string.IsNullOrEmpty(htmlFieldPrefix))
        {
            Prefix = htmlFieldPrefix + "." + Prefix;
        }
    }
}
