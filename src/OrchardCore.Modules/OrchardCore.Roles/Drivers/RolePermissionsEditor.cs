using OrchardCore.Roles.ViewModels;

namespace OrchardCore.Roles.Drivers;

/// <summary>
/// Populates the shape of the permissions table of the role editors.
/// </summary>
internal static class RolePermissionsEditor
{
    public static void Populate(RolePermissionsViewModel source, RolePermissionsViewModel model)
    {
        model.IsAdminRole = source.IsAdminRole;
        model.CanEdit = source.CanEdit;
        model.RoleCategoryPermissions = source.RoleCategoryPermissions;
        model.EffectivePermissions = source.EffectivePermissions;
        model.AssignedPermissions = source.AssignedPermissions;
        model.AnonymousGrantedPermissions = source.AnonymousGrantedPermissions;
        model.AuthenticatedGrantedPermissions = source.AuthenticatedGrantedPermissions;
    }
}
