using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Roles.Workflows.ViewModels;

public class GetUsersByRoleTaskViewModel
{
    [Required]
    public string OutputKeyName { get; set; }

    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
