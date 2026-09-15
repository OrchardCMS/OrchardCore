using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Liquid;

internal sealed class PermissionMigrations : DataMigration
{
    private const string PreviousEditorDescription = "Grants users the ability to edit existing content.";
    private const string EditorDescription = "Grants users the ability to edit content and Liquid templates.";

    private readonly ShellSettings _shellSettings;

    public PermissionMigrations(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public int Create()
    {
        if (!_shellSettings.IsInitializing())
        {
            ShellScope.AddDeferredTask(UpdateEditorRoleAsync);
        }

        return 1;
    }

    private static async Task UpdateEditorRoleAsync(ShellScope shellScope)
    {
        var roleService = shellScope.ServiceProvider.GetService<IRoleService>();
        var roleStore = shellScope.ServiceProvider.GetService<IRoleStore<IRole>>();

        if (roleService is null || roleStore is null)
        {
            return;
        }

        var editor = (await roleService.GetRolesAsync())
            .OfType<Role>()
            .FirstOrDefault(role => string.Equals(
                role.RoleName,
                OrchardCoreConstants.Roles.Editor,
                StringComparison.OrdinalIgnoreCase));

        if (editor is null)
        {
            return;
        }

        var permissionName = Permissions.ManageLiquidTemplates.Name;
        var changed = false;

        if (!editor.RoleClaims.Any(claim =>
            claim.ClaimType == Permission.ClaimType &&
            string.Equals(claim.ClaimValue, permissionName, StringComparison.Ordinal)))
        {
            editor.RoleClaims.Add(RoleClaim.Create(permissionName));
            changed = true;
        }

        if (string.IsNullOrEmpty(editor.RoleDescription) ||
            string.Equals(editor.RoleDescription, PreviousEditorDescription, StringComparison.Ordinal))
        {
            editor.RoleDescription = EditorDescription;
            changed = true;
        }

        if (changed)
        {
            await roleStore.UpdateAsync(editor, CancellationToken.None);
        }
    }
}
