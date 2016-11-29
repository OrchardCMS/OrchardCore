using Microsoft.AspNetCore.Mvc;
using Orchard.OpenId.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.ViewModels
{
    public class EditOpenIdApplicationViewModel
    {
        [HiddenInput]
        public string Id { get; set; }
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
        public List<RoleEntry> RoleEntries { get; set; } = new List<RoleEntry>();
    }

    

}
