using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using Orchard.OpenId.Settings;
using Orchard.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Orchard.OpenId.Services
{
    public class OpenIdService : IOpenIdService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<OpenIdService> T;

        public OpenIdService(ISiteService siteService, IStringLocalizer<OpenIdService> stringLocalizer)
        {
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<OpenIdSettings> GetOpenIdSettingsAsync()
        {
            var settings = await _siteService.GetSiteSettingsAsync();

            var result = settings.As<OpenIdSettings>();
            if (result == null)
            {
                result = new OpenIdSettings();
            }

            return result;
        }

        public async Task UpdateOpenIdSettingsAsync(OpenIdSettings openIdSettings)
        {
            var settings = await _siteService.GetSiteSettingsAsync();
            settings.Properties[nameof(OpenIdSettings)] = JObject.FromObject(openIdSettings);
            await _siteService.UpdateSiteSettingsAsync(settings);
        }

        public bool IsValidOpenIdSettings(OpenIdSettings settings, ModelStateDictionary modelState)
        {
            if (settings == null)
            {
                modelState.AddModelError("", T["Settings are not stablished."]);
                return false;
            }

            if (!settings.AllowAuthorizationCodeFlow && !settings.AllowClientCredentialsFlow 
                && !settings.AllowHybridFlow && !settings.AllowImplicitFlow && !settings.AllowPasswordFlow)
            {
                modelState.AddModelError("", T["At least one OpenID Connect flow must be enabled."]);
                return false;
            }

            ValidateUrisSchema(new string[] { settings.Authority }, !settings.TestingModeEnabled, modelState, "Authority");
            ValidateUrisSchema(settings.Audiences, !settings.TestingModeEnabled, modelState, "Audience");
            
            if (!settings.TestingModeEnabled)
            {
                if (settings.CertificateStoreName == null)
                {
                    modelState.AddModelError("CertificateStoreName", T["A Certificate Store Name is required."]);
                }
                if (settings.CertificateStoreLocation == null)
                {
                    modelState.AddModelError("CertificateStoreLocation", T["A Certificate Store Location is required."]);
                }
                if (string.IsNullOrWhiteSpace(settings.CertificateThumbPrint))
                {
                    modelState.AddModelError("CertificateThumbPrint", T["A certificate is required when testing mode is disabled."]);
                }
                if (!modelState.IsValid)
                    return false;

                var certificate = GetCertificate(settings.CertificateStoreLocation.Value, settings.CertificateStoreName.Value, settings.CertificateThumbPrint);
                if (certificate == null)
                {
                    modelState.AddModelError("CertificateThumbPrint", T["The certificate cannot be found."]);
                    return false;
                }
                if (!certificate.HasPrivateKey)
                {
                    modelState.AddModelError("CertificateThumbPrint", T["The certificate doesn't contain the required private key."]);
                    return false;
                }
                if (certificate.Archived)
                {
                    modelState.AddModelError("CertificateThumbPrint", T["The certificate is not valid because it is marked as archived."]);
                    return false;
                }
                var now = DateTime.Now;
                if (certificate.NotBefore > now || certificate.NotAfter < now)
                {
                    modelState.AddModelError("CertificateThumbPrint", T["The certificate is not valid for current date."]);
                    return false;
                }
            }

            if (settings.AllowPasswordFlow && !settings.EnableTokenEndpoint)
            {
                modelState.AddModelError("AllowPasswordFlow", T["Password Flow cannot be enabled if Token Endpoint is disabled"]);
                return false;
            }
            if (settings.AllowClientCredentialsFlow && !settings.EnableTokenEndpoint)
            {
                modelState.AddModelError("AllowClientCredentialsFlow", T["Client Credentials Flow cannot be enabled if Token Endpoint is disabled"]);
                return false;
            }
            if (settings.AllowAuthorizationCodeFlow && (!settings.EnableAuthorizationEndpoint || !settings.EnableTokenEndpoint))
            {
                modelState.AddModelError("AllowAuthorizationCodeFlow", T["Authorization Code Flow cannot be enabled if Authorization Endpoint and Token Endpoint are disabled"]);
                return false;
            }
            if (settings.AllowHybridFlow && (!settings.EnableAuthorizationEndpoint || !settings.EnableTokenEndpoint))
            {
                modelState.AddModelError("AllowAuthorizationHybridFlow", T["Authorization Hybrid cannot be enabled if Authorization Endpoint and Token Endpoint are disabled"]);
                return false;
            }
            if (settings.AllowRefreshTokenFlow)
            {
                if (!settings.EnableTokenEndpoint)
                {
                    modelState.AddModelError("AllowRefreshTokenFlow", T["Refresh Token Flow cannot be enabled if Token Endpoint is disabled"]);
                    return false;
                }
                if (!settings.AllowPasswordFlow && !settings.AllowAuthorizationCodeFlow && !settings.AllowHybridFlow)
                {
                    modelState.AddModelError("AllowRefreshTokenFlow", T["Refresh Token Flow only can be enabled if Password Flow, Authorization Code Flow or Hybrid Flow are enabled"]);
                    return false;
                }
            }
            if (settings.AllowImplicitFlow && !settings.EnableAuthorizationEndpoint)
            {
                modelState.AddModelError("AllowImplicitFlow", T["Allow Implicit Flow cannot be enabled if Authorization Endpoint is disabled"]);
                return false;
            }
            return modelState.IsValid;
        }

        private void ValidateUrisSchema(IEnumerable<string> uriStrings, bool onlyAllowHttps, ModelStateDictionary modelState, string modelStateKey)
        {
            if (uriStrings == null)
            {
                modelState.AddModelError(modelStateKey, T["Invalid url."]);
                return;
            }
            foreach (var uriString in uriStrings.Select(a => (a ?? "").Trim()))
            {
                Uri uri;
                if (!Uri.TryCreate(uriString, UriKind.Absolute, out uri) || ((onlyAllowHttps || uri.Scheme != "http") && uri.Scheme != "https"))
                {
                    var message = onlyAllowHttps ? T["Invalid url. Non https urls are only allowed in testing mode."] : T["Invalid url."];
                    modelState.AddModelError(modelStateKey, T[message]);
                }
            }
        }

        public bool IsValidOpenIdSettings(OpenIdSettings settings)
        {
            var modelState = new ModelStateDictionary();
            return IsValidOpenIdSettings(settings, modelState);
        }

        public IEnumerable<CertificateInfo> GetAvailableCertificates(bool onlyCertsWithPrivateKey)
        {
            foreach (StoreLocation storeLocation in Enum.GetValues(typeof(StoreLocation)))
            {
                foreach (StoreName storeName in Enum.GetValues(typeof(StoreName)))
                {
                    using (X509Store x509Store = new X509Store(storeName, storeLocation))
                    {
                        yield return new CertificateInfo()
                        {
                            StoreLocation = storeLocation,
                            StoreName = storeName
                        };

                        x509Store.Open(OpenFlags.ReadOnly);

                        X509Certificate2Collection col = x509Store.Certificates;
                        foreach (var cert in col)
                        {
                            if (!cert.Archived && (!onlyCertsWithPrivateKey || (onlyCertsWithPrivateKey && cert.HasPrivateKey)))
                            {
                                yield return new CertificateInfo()
                                {
                                    StoreLocation = storeLocation,
                                    StoreName = storeName,
                                    FriendlyName = cert.FriendlyName,
                                    Issuer = cert.Issuer,
                                    Subject = cert.Subject,
                                    NotBefore = cert.NotBefore,
                                    NotAfter = cert.NotAfter,
                                    ThumbPrint = cert.Thumbprint,
                                    HasPrivateKey = cert.HasPrivateKey,
                                    Archived = cert.Archived
                                };
                            }
                        }
                    }
                }
            }
        }

        public CertificateInfo GetCertificate(StoreLocation storeLocation, StoreName storeName, string certThumbprint)
        {
            using (X509Store x509Store = new X509Store(storeName, storeLocation))
            {
                x509Store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection col = x509Store.Certificates;
                foreach (var cert in col)
                {
                    if (cert.Thumbprint == certThumbprint)
                    {
                        return new CertificateInfo()
                        {
                            StoreLocation = storeLocation,
                            StoreName = storeName,
                            FriendlyName = cert.FriendlyName,
                            Issuer = cert.Issuer,
                            Subject = cert.Subject,
                            NotBefore = cert.NotBefore,
                            NotAfter = cert.NotAfter,
                            ThumbPrint = cert.Thumbprint,
                            HasPrivateKey = cert.HasPrivateKey,
                            Archived = cert.Archived
                        };
                    }
                }
            }
            return null;
        }
    }
}
