using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Security;

namespace OrchardCore.Roles.ViewModels;

public class EditRoleViewModel
{
    public string RoleDescription { get; set; }

    [BindNever]
    public Role Role { get; set; }

    [BindNever]
    public RolePermissionsViewModel Permissions { get; set; }
}
