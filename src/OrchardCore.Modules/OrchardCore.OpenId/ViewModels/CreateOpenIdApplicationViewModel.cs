using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OrchardCore.OpenId.Models;

namespace OrchardCore.OpenId.ViewModels
{
    public class CreateOpenIdApplicationViewModel
    {
        [Required]
        public string ClientId { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Url]
        public string RedirectUri { get; set; }
        [Url]
        public string LogoutRedirectUri { get; set; }
        public ClientType Type { get; set; }
        public bool SkipConsent { get; set; }
        [DataType(DataType.Password)]
        public string ClientSecret { get; set; }
        [DataType(DataType.Password)]
        [Compare("ClientSecret")]
        public string ConfirmClientSecret { get; set; }
        public List<RoleEntry> RoleEntries { get; } = new List<RoleEntry>();
        public bool AllowPasswordFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
    }

    public class RoleEntry
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
}
