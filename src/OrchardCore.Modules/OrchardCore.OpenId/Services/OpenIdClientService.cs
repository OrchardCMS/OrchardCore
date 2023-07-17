using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.OpenId.Settings;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Services
{
    public class OpenIdClientService : IOpenIdClientService
    {
        private readonly ISiteService _siteService;
        protected readonly IStringLocalizer S;

        public OpenIdClientService(
            ISiteService siteService,
            IStringLocalizer<OpenIdClientService> stringLocalizer)
        {
            _siteService = siteService;
            S = stringLocalizer;
        }

        public async Task<OpenIdClientSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<OpenIdClientSettings>();
        }

        public async Task<OpenIdClientSettings> LoadSettingsAsync()
        {
            var container = await _siteService.LoadSiteSettingsAsync();
            return container.As<OpenIdClientSettings>();
        }

        public async Task UpdateSettingsAsync(OpenIdClientSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.LoadSiteSettingsAsync();
            container.Properties[nameof(OpenIdClientSettings)] = JObject.FromObject(settings);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public Task<ImmutableArray<ValidationResult>> ValidateSettingsAsync(OpenIdClientSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var results = ImmutableArray.CreateBuilder<ValidationResult>();

            if (settings.Authority == null)
            {
                results.Add(new ValidationResult(S["The authority cannot be null or empty."], new[]
                {
                    nameof(settings.Authority)
                }));
            }
            else if (!settings.Authority.IsAbsoluteUri || !settings.Authority.IsWellFormedOriginalString())
            {
                results.Add(new ValidationResult(S["The authority must be a valid absolute URL."], new[]
                {
                    nameof(settings.Authority)
                }));
            }
            else if (!String.IsNullOrEmpty(settings.Authority.Query) || !String.IsNullOrEmpty(settings.Authority.Fragment))
            {
                results.Add(new ValidationResult(S["The authority cannot contain a query string or a fragment."], new[]
                {
                    nameof(settings.Authority)
                }));
            }

            if (String.IsNullOrEmpty(settings.ResponseType))
            {
                results.Add(new ValidationResult(S["The response type cannot be null or empty."], new[]
                {
                    nameof(settings.ResponseType)
                }));
            }
            else if (settings.ResponseType != OpenIdConnectResponseType.Code && settings.ResponseType != OpenIdConnectResponseType.CodeIdToken &&
                settings.ResponseType != OpenIdConnectResponseType.CodeIdTokenToken && settings.ResponseType != OpenIdConnectResponseType.CodeToken &&
                settings.ResponseType != OpenIdConnectResponseType.IdToken && settings.ResponseType != OpenIdConnectResponseType.IdTokenToken)
            {
                results.Add(new ValidationResult(S["Unknown response type ."], new[]
                {
                    nameof(settings.ResponseType)
                }));
            }

            if (String.IsNullOrEmpty(settings.ResponseMode))
            {
                results.Add(new ValidationResult(S["The response mode cannot be null or empty."], new[]
                {
                    nameof(settings.ResponseMode)
                }));
            }
            else if (settings.ResponseMode != OpenIdConnectResponseMode.FormPost && settings.ResponseMode != OpenIdConnectResponseMode.Fragment &&
                settings.ResponseMode != OpenIdConnectResponseMode.Query)
            {
                results.Add(new ValidationResult(S["Unknown response mode."], new[]
                {
                    nameof(settings.ResponseMode)
                }));
            }

            return Task.FromResult(results.ToImmutable());
        }
    }
}
