using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OpenIddict.Server;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Settings;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Services
{
    public class OpenIdValidationService : IOpenIdValidationService
    {
        private readonly ShellDescriptor _shellDescriptor;
        private readonly ShellSettings _shellSettings;
        private readonly IShellHost _shellHost;
        private readonly ISiteService _siteService;
        protected readonly IStringLocalizer S;

        public OpenIdValidationService(
            ShellDescriptor shellDescriptor,
            ShellSettings shellSettings,
            IShellHost shellHost,
            ISiteService siteService,
            IStringLocalizer<OpenIdValidationService> stringLocalizer)
        {
            _shellDescriptor = shellDescriptor;
            _shellSettings = shellSettings;
            _shellHost = shellHost;
            _siteService = siteService;
            S = stringLocalizer;
        }

        public async Task<OpenIdValidationSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return GetSettingsFromContainer(container);
        }

        public async Task<OpenIdValidationSettings> LoadSettingsAsync()
        {
            var container = await _siteService.LoadSiteSettingsAsync();
            return GetSettingsFromContainer(container);
        }

        private OpenIdValidationSettings GetSettingsFromContainer(ISite container)
        {
            if (container.Properties.TryGetValue(nameof(OpenIdValidationSettings), out var settings))
            {
                return settings.ToObject<OpenIdValidationSettings>();
            }

            // If the OpenID validation settings haven't been populated yet, assume the validation
            // feature will use the OpenID server registered in this tenant if it's been enabled.
            if (_shellDescriptor.Features.Any(feature => feature.Id == OpenIdConstants.Features.Server))
            {
                return new OpenIdValidationSettings
                {
                    Tenant = _shellSettings.Name,
                };
            }

            return new OpenIdValidationSettings();
        }

        public async Task UpdateSettingsAsync(OpenIdValidationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.LoadSiteSettingsAsync();
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

            if (!(settings.Authority == null ^ String.IsNullOrEmpty(settings.Tenant)))
            {
                results.Add(new ValidationResult(S["Either a tenant or an authority must be registered."], new[]
                {
                    nameof(settings.Authority),
                    nameof(settings.Tenant),
                }));
            }

            if (settings.Authority != null)
            {
                if (!settings.Authority.IsAbsoluteUri || !settings.Authority.IsWellFormedOriginalString())
                {
                    results.Add(new ValidationResult(S["The specified authority is not valid."], new[]
                    {
                        nameof(settings.Authority)
                    }));
                }

                if (!String.IsNullOrEmpty(settings.Authority.Query) || !String.IsNullOrEmpty(settings.Authority.Fragment))
                {
                    results.Add(new ValidationResult(S["The authority cannot contain a query string or a fragment."], new[]
                    {
                        nameof(settings.Authority),
                    }));
                }

            }

            if (settings.MetadataAddress != null)
            {
                if (!settings.MetadataAddress.IsAbsoluteUri || !settings.MetadataAddress.IsWellFormedOriginalString())
                {
                    results.Add(new ValidationResult(S["The specified metadata address is not valid."], new[]
                    {
                        nameof(settings.MetadataAddress),
                    }));
                }

                if (!String.IsNullOrEmpty(settings.MetadataAddress.Query) || !String.IsNullOrEmpty(settings.MetadataAddress.Fragment))
                {
                    results.Add(new ValidationResult(S["The metadata address cannot contain a query string or a fragment."], new[]
                    {
                        nameof(settings.MetadataAddress),
                    }));
                }

                if (!String.IsNullOrEmpty(settings.Tenant))
                {
                    results.Add(new ValidationResult(S["No metatada address can be set when using another tenant."], new[]
                    {
                        nameof(settings.MetadataAddress),
                    }));
                }
            }

            if (!String.IsNullOrEmpty(settings.Tenant) && !String.IsNullOrEmpty(settings.Audience))
            {
                results.Add(new ValidationResult(S["No audience can be set when using another tenant."], new[]
                {
                    nameof(settings.Audience),
                }));
            }

            if (settings.Authority != null && String.IsNullOrEmpty(settings.Audience))
            {
                results.Add(new ValidationResult(S["An audience must be set when configuring the authority."], new[]
                {
                    nameof(settings.Audience),
                }));
            }

            if (settings.Authority == null && settings.DisableTokenTypeValidation)
            {
                results.Add(new ValidationResult(S["Token type validation can only be disabled for remote servers."], new[]
                {
                    nameof(settings.DisableTokenTypeValidation),
                }));
            }

            if (!String.IsNullOrEmpty(settings.Audience) &&
                settings.Audience.StartsWith(OpenIdConstants.Prefixes.Tenant, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new ValidationResult(S["The audience cannot start with the special 'oct:' prefix."], new[]
                {
                    nameof(settings.Audience),
                }));
            }

            // If a tenant was specified, ensure it is valid, that the OpenID server feature
            // was enabled and that at least a scope linked with the current tenant exists.
            if (!String.IsNullOrEmpty(settings.Tenant) &&
                !String.Equals(settings.Tenant, _shellSettings.Name))
            {
                if (!_shellHost.TryGetSettings(settings.Tenant, out var shellSettings))
                {
                    results.Add(new ValidationResult(S["The specified tenant is not valid."]));
                }
                else
                {
                    var shellScope = await _shellHost.GetScopeAsync(shellSettings);

                    await shellScope.UsingAsync(async scope =>
                    {
                        var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<OpenIddictServerOptions>>().CurrentValue;
                        if (options.UseReferenceAccessTokens)
                        {
                            results.Add(new ValidationResult(S["Selecting a server tenant for which reference access tokens are enabled is currently not supported."], new[]
                            {
                                nameof(settings.Tenant),
                            }));
                        }

                        var manager = scope.ServiceProvider.GetService<IOpenIdScopeManager>();
                        if (manager == null)
                        {
                            results.Add(new ValidationResult(S["The specified tenant is not valid."], new[]
                            {
                                nameof(settings.Tenant),
                            }));
                        }
                        else
                        {
                            var resource = OpenIdConstants.Prefixes.Tenant + _shellSettings.Name;
                            if (!await manager.FindByResourceAsync(resource).AnyAsync())
                            {
                                results.Add(new ValidationResult(S["No appropriate scope was found."], new[]
                                {
                                    nameof(settings.Tenant),
                                }));
                            }
                        }
                    });
                }
            }

            return results.ToImmutable();
        }
    }
}
