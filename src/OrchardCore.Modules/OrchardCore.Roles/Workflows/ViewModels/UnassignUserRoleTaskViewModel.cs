using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Roles.Workflows.ViewModels;

public class UnassignUserRoleTaskViewModel
{
    [Required]
    public string UserName { get; set; }

    [Required]
    public IEnumerable<string> Roles { get; set; }
}
