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

        [Required]
        [Url]
        public string RedirectUri { get; set; }

        [Required]
        [Url]
        public string LogoutRedirectUri { get; set; }
        
        public ClientType Type { get; set; }

        public bool SkipConsent { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        public List<RoleEntry> RoleEntries { get; set; } = new List<RoleEntry>();
    }
    
    public class RoleEntry
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
    }


}
