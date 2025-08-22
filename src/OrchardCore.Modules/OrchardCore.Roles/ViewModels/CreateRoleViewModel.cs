using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Roles.ViewModels;

public class CreateRoleViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string RoleName { get; set; }

    public string RoleDescription { get; set; }
}
