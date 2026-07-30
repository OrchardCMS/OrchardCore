using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.ViewModels;

public class RolePermissionsViewModel
{
    [BindNever]
    public bool IsAdminRole { get; set; }

    [BindNever]
    public bool CanEdit { get; set; }

    [BindNever]
    public IDictionary<PermissionGroupKey, IEnumerable<Permission>> RoleCategoryPermissions { get; set; }

    [BindNever]
    public IEnumerable<string> EffectivePermissions { get; set; }

    [BindNever]
    public ISet<string> AssignedPermissions { get; set; }
}
