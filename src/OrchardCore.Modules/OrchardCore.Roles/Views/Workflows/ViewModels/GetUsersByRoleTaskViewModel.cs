using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Roles.Workflows.ViewModels;

public class GetUsersByRoleTaskViewModel
{
    [Required]
    public string OutputKeyName { get; set; }

    [Required]
    public IEnumerable<string> Roles { get; set; }
}
