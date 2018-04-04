using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.OpenId.ViewModels
{
    public class CreateOpenIdScopeViewModel
    {
        public string Description { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string Name { get; set; }

        public string Resources { get; set; }

        public List<TenantEntry> Tenants { get; } = new List<TenantEntry>();

        public class TenantEntry
        {
            public bool Current { get; set; }
            public string Name { get; set; }
            public bool Selected { get; set; }
        }
    }
}
