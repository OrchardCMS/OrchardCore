using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.OpenId.ViewModels
{
    public class CreateOpenIdApplicationViewModel
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Url(ErrorMessage = "{0} is not well-formed")]
        public string RedirectUris { get; set; }

        [Url(ErrorMessage = "{0} is not well-formed")]
        public string PostLogoutRedirectUris { get; set; }

        public string Type { get; set; }

        public string ConsentType { get; set; }

        public string ClientSecret { get; set; }

        public List<RoleEntry> RoleEntries { get; } = new List<RoleEntry>();

        public List<ScopeEntry> ScopeEntries { get; } = new List<ScopeEntry>();

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
            public bool Selected { get; set; }
        }
        public class ScopeEntry
        {
            public string Name { get; set; }
            public bool Selected { get; set; }
        }
    }
}
