using Orchard.OpenId.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.ViewModels
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
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string ClientSecret { get; set; }
        [DataType(DataType.Password)]
        [Compare("ClientSecret")]
        public string ConfirmClientSecret { get; set; }
        public bool AllowPasswordFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
        public bool AllowHybridFlow { get; set; }
    }


}
