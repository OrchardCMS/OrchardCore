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
        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string ClientSecret { get; set; }
        [DataType(DataType.Password)]
        [Compare("ClientSecret")]
        public string ConfirmClientSecret { get; set; }
        public List<RoleEntry> RoleEntries { get; set; } = new List<RoleEntry>();
    }
    
    public class RoleEntry
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
    }


}
