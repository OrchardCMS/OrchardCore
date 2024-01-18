using System;

namespace OrchardCore.OpenId.Recipes
{
    public class OpenIdApplicationStepModel
    {
        public string ClientId { get; set; }
        public string DisplayName { get; set; }
        public string RedirectUris { get; set; }
        public string PostLogoutRedirectUris { get; set; }
        public string Type { get; set; }
        public string ConsentType { get; set; }
        public string ClientSecret { get; set; }
        public RoleEntry[] RoleEntries { get; set; } = Array.Empty<RoleEntry>();
        public ScopeEntry[] ScopeEntries { get; set; } = Array.Empty<ScopeEntry>();
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

        public class RoleEntry
        {
            public string Name { get; set; }
        }

        public class ScopeEntry
        {
            public string Name { get; set; }
        }
    }
}
