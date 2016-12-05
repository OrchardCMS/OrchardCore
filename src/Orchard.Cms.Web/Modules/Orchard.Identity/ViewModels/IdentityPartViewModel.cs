using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Identity.Models;
using Orchard.Identity.Settings;

namespace Orchard.Identity.ViewModels
{
    public class IdentityPartViewModel
    {
        public string Name { get; set; }
        public string Identity { get; set; }

        [BindNever]
        public IdentityPart IdentityPart { get; set; }

        [BindNever]
        public IdentityPartSettings IdentityPartSettings { get; set; }
    }
}
