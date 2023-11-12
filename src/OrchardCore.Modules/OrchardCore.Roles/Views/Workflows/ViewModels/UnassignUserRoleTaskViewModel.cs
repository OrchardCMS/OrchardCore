using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Roles.Workflows.ViewModels;

public class UnassignUserRoleTaskViewModel
{
    [Required]
    public string UserName { get; set; }

    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
