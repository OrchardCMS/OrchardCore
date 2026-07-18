using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Security;

namespace OrchardCore.Roles.ViewModels;

public class DisplayRoleViewModel
{
    [BindNever]
    public Role Role { get; set; }

    [BindNever]
    public RolePermissionsViewModel Permissions { get; set; }
}
