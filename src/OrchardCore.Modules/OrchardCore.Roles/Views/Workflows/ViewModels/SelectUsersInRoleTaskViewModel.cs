using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Roles.Workflows.ViewModels
{
    public class SelectUsersInRoleTaskViewModel
    {
        [Required]
        public string OutputKeyName { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
