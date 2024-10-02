using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.ViewModels;

public class EditRoleViewModel
{
    public string Name { get; set; }

    public string RoleDescription { get; set; }

    [BindNever]
    public bool IsAdminRole { get; set; }

    [BindNever]
    public IDictionary<PermissionGroupKey, IEnumerable<Permission>> RoleCategoryPermissions { get; set; }

    [BindNever]
    public IEnumerable<string> EffectivePermissions { get; set; }

    [BindNever]
    public Role Role { get; set; }
}
