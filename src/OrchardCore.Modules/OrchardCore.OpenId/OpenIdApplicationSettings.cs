using System;
using System.Linq;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;

namespace OrchardCore.OpenId
{
    public class OpenIdApplicationSettings
    {
        public string ClientId { get; set; }
        public string DisplayName { get; set; }
        public string RedirectUris { get; set; }
        public string PostLogoutRedirectUris { get; set; }
        public string Type { get; set; }
        public string ConsentType { get; set; }
        public string ClientSecret { get; set; }
        public string[] Roles { get; set; }
        public string[] Scopes { get; set; }
        public bool AllowPasswordFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowHybridFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
        public bool AllowLogoutEndpoint { get; set; }
        public bool AllowIntrospectionEndpoint { get; set; }
        public bool AllowRevocationEndpoint { get; set; }
        public bool RequireProofKeyForCodeExchange { get; set; }
    }

    internal static class OpenIdApplicationExtensions
    {
        public static async Task UpdateDescriptorFromSettings(this IOpenIdApplicationManager _applicationManager, OpenIdApplicationSettings model, object application = null)
        {
            var descriptor = new OpenIdApplicationDescriptor();

            if (application != null)
            {
                await _applicationManager.PopulateAsync(descriptor, application);
            }

            descriptor.ClientId = model.ClientId;
            descriptor.ConsentType = model.ConsentType;
            descriptor.DisplayName = model.DisplayName;
            descriptor.Type = model.Type;

            if (!string.IsNullOrEmpty(model.ClientSecret))
            {
                descriptor.ClientSecret = model.ClientSecret;
            }

            if (string.Equals(descriptor.Type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
            {
                descriptor.ClientSecret = null;
            }

            if (model.AllowLogoutEndpoint)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Logout);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Logout);
            }

            if (model.AllowAuthorizationCodeFlow || model.AllowHybridFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            }

            if (model.AllowClientCredentialsFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
            }

            if (model.AllowHybridFlow || model.AllowImplicitFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Implicit);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.Implicit);
            }

            if (model.AllowPasswordFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.Password);
            }

            if (model.AllowRefreshTokenFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            }

            if (model.AllowAuthorizationCodeFlow || model.AllowHybridFlow || model.AllowImplicitFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Authorization);
            }

            if (model.AllowAuthorizationCodeFlow || model.AllowHybridFlow ||
                model.AllowClientCredentialsFlow || model.AllowPasswordFlow || model.AllowRefreshTokenFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Token);
            }

            if (model.AllowAuthorizationCodeFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.Code);
            }

            if (model.AllowImplicitFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdToken);

                if (string.Equals(model.Type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                {
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Token);
                }
                else
                {
                    descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                    descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.Token);
                }
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.IdToken);
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.Token);
            }

            if (model.AllowHybridFlow)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);

                if (string.Equals(model.Type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                {
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                    descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
                }
                else
                {
                    descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                    descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
                }
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
            }

            if (model.AllowIntrospectionEndpoint)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Introspection);
            }

            if (model.AllowRevocationEndpoint)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Revocation);
            }

            if (model.RequireProofKeyForCodeExchange)
            {
                descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
            }
            else
            {
                descriptor.Requirements.Remove(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
            }

            descriptor.Roles.Clear();

            foreach (var role in model.Roles)
            {
                descriptor.Roles.Add(role);
            }

            descriptor.Permissions.RemoveWhere(permission => permission.StartsWith(OpenIddictConstants.Permissions.Prefixes.Scope));
            foreach (var scope in model.Scopes)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
            }

            descriptor.PostLogoutRedirectUris.Clear();
            foreach (Uri uri in
                (from uri in model.PostLogoutRedirectUris?.Split(new[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>()
                 select new Uri(uri, UriKind.Absolute)))
            {
                descriptor.PostLogoutRedirectUris.Add(uri);
            }

            descriptor.RedirectUris.Clear();
            foreach (Uri uri in
               (from uri in model.RedirectUris?.Split(new[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>()
                select new Uri(uri, UriKind.Absolute)))
            {
                descriptor.RedirectUris.Add(uri);
            }

            if (application == null)
            {
                await _applicationManager.CreateAsync(descriptor);
            }
            else
            {
                await _applicationManager.UpdateAsync(application, descriptor);
            }

        }
    }
}
