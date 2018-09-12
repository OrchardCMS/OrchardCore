using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Settings;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Services
{
    public class OpenIdValidationService : IOpenIdValidationService
    {
        private readonly IMemoryCache _cache;
        private readonly ShellSettings _shellSettings;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellHost _shellHost;
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<OpenIdValidationService> T;

        public OpenIdValidationService(
            IMemoryCache cache,
            ShellSettings shellSettings,
            IShellSettingsManager shellSettingsManager,
            IShellHost shellHost,
            ISiteService siteService,
            IStringLocalizer<OpenIdValidationService> stringLocalizer)
        {
            _cache = cache;
            _shellSettings = shellSettings;
            _shellSettingsManager = shellSettingsManager;
            _shellHost = shellHost;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<OpenIdValidationSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<OpenIdValidationSettings>();
        }

        public async Task UpdateSettingsAsync(OpenIdValidationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.GetSiteSettingsAsync();
            container.Properties[nameof(OpenIdValidationSettings)] = JObject.FromObject(settings);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public async Task<ImmutableArray<ValidationResult>> ValidateSettingsAsync(OpenIdValidationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var results = ImmutableArray.CreateBuilder<ValidationResult>();

            if (!(string.IsNullOrEmpty(settings.Authority) ^ string.IsNullOrEmpty(settings.Tenant)))
            {
                results.Add(new ValidationResult(T["Either a tenant or an authority must be registered."], new[]
                {
                    nameof(settings.Authority),
                    nameof(settings.Tenant)
                }));
            }

            if (!string.IsNullOrEmpty(settings.Authority))
            {
                if (!Uri.TryCreate(settings.Authority, UriKind.Absolute, out Uri uri) || !uri.IsWellFormedOriginalString())
                {
                    results.Add(new ValidationResult(T["The specified authority is not valid."], new[]
                    {
                        nameof(settings.Authority)
                    }));
                }

                if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
                {
                    results.Add(new ValidationResult(T["The authority cannot contain a query string or a fragment."], new[]
                    {
                        nameof(settings.Authority)
                    }));
                }
            }

            if (string.IsNullOrEmpty(settings.Authority) && !string.IsNullOrEmpty(settings.Audience))
            {
                results.Add(new ValidationResult(T["No audience can be set when using another tenant."], new[]
                {
                    nameof(settings.Audience)
                }));
            }

            if (!string.IsNullOrEmpty(settings.Authority) && string.IsNullOrEmpty(settings.Audience))
            {
                results.Add(new ValidationResult(T["An audience must be set when configuring the authority."], new[]
                {
                    nameof(settings.Audience)
                }));
            }

            if (!string.IsNullOrEmpty(settings.Audience) &&
                settings.Audience.StartsWith(OpenIdConstants.Prefixes.Tenant, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new ValidationResult(T["The audience cannot start with the special 'oct:' prefix."], new[]
                {
                    nameof(settings.Audience)
                }));
            }

            // If a tenant was specified, ensure it is valid, that the OpenID server feature
            // was enabled and that at least a scope linked with the current tenant exists.
            if (!string.IsNullOrEmpty(settings.Tenant) &&
                !string.Equals(settings.Tenant, _shellSettings.Name, StringComparison.Ordinal))
            {
                if (!_shellSettingsManager.TryGetSettings(settings.Tenant, out var tenant))
                {
                    results.Add(new ValidationResult(T["The specified tenant is not valid."]));
                }
                else
                {
                    using (var scope = await _shellHost.GetScopeAsync(tenant))
                    {
                        var manager = scope.ServiceProvider.GetService<IOpenIdScopeManager>();
                        if (manager == null)
                        {
                            results.Add(new ValidationResult(T["The specified tenant is not valid."], new[]
                            {
                                nameof(settings.Tenant)
                            }));
                        }
                        else
                        {
                            var resource = OpenIdConstants.Prefixes.Tenant + _shellSettings.Name;
                            var scopes = await manager.FindByResourceAsync(resource);
                            if (scopes.IsDefaultOrEmpty)
                            {
                                results.Add(new ValidationResult(T["No appropriate scope was found."], new[]
                                {
                                    nameof(settings.Tenant)
                                }));
                            }
                        }
                    }
                }
            }

            return results.ToImmutable();
        }
    }
}
